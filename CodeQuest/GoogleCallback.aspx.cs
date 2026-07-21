using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using CodeQuest.Data;
using CodeQuest.Data.Repositories;
using CodeQuest.Models;

namespace CodeQuest
{
    public partial class GoogleCallback : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CompleteSignIn();
            }
        }

        private void CompleteSignIn()
        {
            string expectedState = Session[Login.GoogleOAuthStateSessionKey] as string;
            Session.Remove(Login.GoogleOAuthStateSessionKey);

            if (!GoogleOAuthClient.IsConfigured)
            {
                Fail("Google sign-in is not configured yet.");
                return;
            }

            string state = Request.QueryString["state"];
            if (!SafeEquals(expectedState, state))
            {
                Fail("Google sign-in could not be verified. Please try again.");
                return;
            }

            string error = Request.QueryString["error"];
            if (!string.IsNullOrWhiteSpace(error))
            {
                Fail(string.Equals(error, "access_denied", StringComparison.OrdinalIgnoreCase)
                    ? "Google sign-in was cancelled."
                    : "Google sign-in could not be completed.");
                return;
            }

            try
            {
                string code = Request.QueryString["code"];
                GoogleProfile profile = new GoogleOAuthClient().ExchangeCode(
                    code,
                    GoogleOAuthClient.GetRedirectUri(Request));

                UserRecord user = FindOrCreateUser(profile);
                SignIn(user);
                RedirectAfterSignIn(user.Role);
            }
            catch (SqlException)
            {
                Fail("Google sign-in succeeded, but CodeQuestDB could not save or load your account.");
            }
            catch (InvalidOperationException exception)
            {
                Fail(exception.Message);
            }
        }

        private static UserRecord FindOrCreateUser(GoogleProfile profile)
        {
            UserRepository repository = new UserRepository();
            UserRecord user = repository.FindByGoogleID(profile.Subject);
            if (user != null)
            {
                return user;
            }

            user = repository.FindByEmail(profile.Email);
            if (user != null)
            {
                if (!string.IsNullOrWhiteSpace(user.GoogleID)
                    && !string.Equals(user.GoogleID, profile.Subject, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("This CodeQuest email is already linked to another Google account.");
                }

                if (!repository.LinkGoogleID(user.UserID, profile.Subject))
                {
                    throw new InvalidOperationException("The Google account could not be linked to this CodeQuest account.");
                }

                user.GoogleID = profile.Subject;
                return user;
            }

            string username = CreateAvailableUsername(repository, profile);
            string unusablePasswordHash = PasswordHasher.Hash(Guid.NewGuid().ToString("N"));
            int userID = repository.CreateGoogleLearner(username, unusablePasswordHash, profile.Email, profile.Subject);
            return repository.FindByID(userID);
        }

        private static string CreateAvailableUsername(UserRepository repository, GoogleProfile profile)
        {
            string baseName = ToUsername(profile.Name);
            if (string.IsNullOrWhiteSpace(baseName))
            {
                baseName = ToUsername(profile.Email.Split('@')[0]);
            }

            if (string.IsNullOrWhiteSpace(baseName))
            {
                baseName = "googlelearner";
            }

            baseName = baseName.Length > 40 ? baseName.Substring(0, 40) : baseName;
            string candidate = baseName;
            int suffix = 1;
            while (!repository.IsUsernameAvailable(candidate))
            {
                string suffixText = suffix.ToString();
                int maxBaseLength = 50 - suffixText.Length;
                string shortened = baseName.Length > maxBaseLength
                    ? baseName.Substring(0, maxBaseLength)
                    : baseName;
                candidate = shortened + suffixText;
                suffix++;
            }

            return candidate;
        }

        private static string ToUsername(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            foreach (char character in value.Trim().ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(character);
                }
                else if (builder.Length > 0 && builder[builder.Length - 1] != '_')
                {
                    builder.Append('_');
                }
            }

            return builder.ToString().Trim('_');
        }

        private void SignIn(UserRecord user)
        {
            if (user == null)
            {
                throw new InvalidOperationException("The CodeQuest account could not be loaded after Google sign-in.");
            }

            Session["DisplayName"] = user.Username;
            Session["UserEmail"] = user.Email;
            Session["UserRole"] = user.Role;
            Session["UserID"] = user.UserID;
            Session["UserPlan"] = user.Plan;
        }

        private void RedirectAfterSignIn(string role)
        {
            string returnUrl = Session["ReturnUrl"] == null ? null : Session["ReturnUrl"].ToString();
            if (IsSafeLocalReturnUrl(returnUrl) && IsReturnUrlAllowedForRole(returnUrl, role))
            {
                Session.Remove("ReturnUrl");
                Response.Redirect(returnUrl, false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            Session.Remove("ReturnUrl");
            string destination = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
                ? "AdminDashboard.aspx"
                : "LearnerDashboard.aspx";
            Response.Redirect(destination, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private static bool IsReturnUrlAllowedForRole(string returnUrl, string role)
        {
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return returnUrl.IndexOf("AdminDashboard.aspx", StringComparison.OrdinalIgnoreCase) >= 0
                    || returnUrl.IndexOf("/Features/Admin/", StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return returnUrl.IndexOf("AdminDashboard.aspx", StringComparison.OrdinalIgnoreCase) < 0
                && returnUrl.IndexOf("/Features/Admin/", StringComparison.OrdinalIgnoreCase) < 0;
        }

        private static bool IsSafeLocalReturnUrl(string returnUrl)
        {
            return !string.IsNullOrWhiteSpace(returnUrl)
                && returnUrl.StartsWith("/", StringComparison.Ordinal)
                && !returnUrl.StartsWith("//", StringComparison.Ordinal)
                && returnUrl.IndexOf("://", StringComparison.Ordinal) < 0;
        }

        private void Fail(string message)
        {
            Session["LoginMessage"] = message;
            Response.Redirect("Login.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private static bool SafeEquals(string first, string second)
        {
            if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(second))
            {
                return false;
            }

            byte[] firstBytes = Encoding.UTF8.GetBytes(first);
            byte[] secondBytes = Encoding.UTF8.GetBytes(second);
            uint difference = (uint)firstBytes.Length ^ (uint)secondBytes.Length;
            int length = Math.Min(firstBytes.Length, secondBytes.Length);
            for (int index = 0; index < length; index++)
            {
                difference |= (uint)(firstBytes[index] ^ secondBytes[index]);
            }

            return difference == 0;
        }
    }
}
