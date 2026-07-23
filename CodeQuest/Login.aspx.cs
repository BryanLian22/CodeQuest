// Purpose: Authenticates local and demo administrator credentials, establishes sessions and applies safe redirects.
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using CodeQuest.Data;
using CodeQuest.Data.Repositories;
using CodeQuest.Models;

namespace CodeQuest
{
    public partial class Login : System.Web.UI.Page
    {
        internal const string GoogleOAuthStateSessionKey = "CodeQuestGoogleOAuthState";

        // Temporary administrator fallback for marking and demonstration.
        // Normal learner authentication always uses the SQL Server user record.
        private const string AdminEmail = "admin@codequest.io";
        private const string AdminPassword = "Admin123!";

        // Configure UTF-8 before the page writes any text to the response.
        protected override void OnPreInit(EventArgs e)
        {
            Response.ContentEncoding = Encoding.UTF8;
            Response.Charset = "utf-8";
            base.OnPreInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.Equals(Request.QueryString["logout"], "1", StringComparison.Ordinal))
            {
                Session.Clear();
                Session.Abandon();
                Response.Redirect("Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (IsPostBack)
            {
                return;
            }

            if (Session["UserRole"] != null)
            {
                RedirectAfterSignIn(Session["UserRole"].ToString());
                return;
            }

            if (Session["LoginMessage"] != null)
            {
                ShowMessage(Session["LoginMessage"].ToString(), "success");
                Session.Remove("LoginMessage");
            }

            if (Session["RegisteredEmail"] != null)
            {
                txtEmail.Text = Session["RegisteredEmail"].ToString();
            }
            else if (Request.Cookies["CodeQuestEmail"] != null)
            {
                txtEmail.Text = Request.Cookies["CodeQuestEmail"].Value;
                chkRememberMe.Checked = true;
            }
        }

        // Authenticate database accounts first, then allow the administrator-only demo fallback.
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;
            UserRecord databaseUser = null;

            try
            {
                databaseUser = new UserRepository().FindByEmail(email);
                if (databaseUser != null && PasswordHasher.Verify(password, databaseUser.PasswordHash))
                {
                    SignIn(databaseUser.Username, databaseUser.Email, databaseUser.Role, databaseUser.UserID, databaseUser.Plan);
                    RedirectAfterSignIn(databaseUser.Role);
                    return;
                }
            }
            catch (ConfigurationErrorsException)
            {
                ShowMessage("The database connection is not configured yet. The demo administrator account is still available.", "info");
            }
            catch (SqlException)
            {
                ShowMessage("The database could not be reached. The demo administrator account is still available.", "info");
            }

            if (email.Equals(AdminEmail, StringComparison.OrdinalIgnoreCase) &&
                password == AdminPassword)
            {
                if (databaseUser != null && string.Equals(databaseUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    SignIn(databaseUser.Username, databaseUser.Email, databaseUser.Role, databaseUser.UserID, databaseUser.Plan);
                }
                else
                {
                    SignIn("Administrator", AdminEmail, "Admin");
                }
                RedirectAfterSignIn("Admin");
                return;
            }

            if (IsRegisteredLearner(email, password))
            {
                string username = Session["RegisteredUsername"] == null
                    ? "Learner"
                    : Session["RegisteredUsername"].ToString();

                SignIn(username, email, "Learner");
                RedirectAfterSignIn("Learner");
                return;
            }

            ShowMessage("The email address or password is incorrect.", "error");
        }

        // Start Google OAuth with a session-bound state value to prevent forged callbacks.
        protected void btnGoogle_Click(object sender, EventArgs e)
        {
            if (!GoogleOAuthClient.IsConfigured)
            {
                ShowMessage("Google sign-in is not configured yet. Add CodeQuestGoogleClientId and CodeQuestGoogleClientSecret to Web.config.", "info");
                return;
            }

            string state = GoogleOAuthClient.CreateState();
            Session[GoogleOAuthStateSessionKey] = state;
            string redirectUri = GoogleOAuthClient.GetRedirectUri(Request);
            string authorizationUrl = GoogleOAuthClient.BuildAuthorizationUrl(state, redirectUri);

            Response.Redirect(authorizationUrl, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        // Store only the minimum identity and authorization data required by protected pages.
        private void SignIn(string displayName, string email, string role)
        {
            SignIn(displayName, email, role, null, null);
        }

        private void SignIn(string displayName, string email, string role, int? userID, string plan)
        {
            Session["DisplayName"] = displayName;
            Session["UserEmail"] = email;
            Session["UserRole"] = role;
            Session["UserID"] = userID;
            Session["UserPlan"] = plan;

            if (chkRememberMe.Checked)
            {
                // This non-sensitive cookie remembers only the email address.
                // Authentication must remain in the server-side session.
                Response.Cookies["CodeQuestEmail"].Value = email;
                Response.Cookies["CodeQuestEmail"].Expires = DateTime.Now.AddDays(14);
                Response.Cookies["CodeQuestEmail"].HttpOnly = true;
                Response.Cookies["CodeQuestEmail"].Secure = Request.IsSecureConnection;
            }
        }

        private void RedirectByRole(string role)
        {
            Session.Remove("ReturnUrl");
            string destination = role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                ? "AdminDashboard.aspx"
                : "LearnerDashboard.aspx";

            Response.Redirect(destination, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void RedirectAfterSignIn(string role)
        {
            string returnUrl = Session["ReturnUrl"] == null
                ? null
                : Session["ReturnUrl"].ToString();

            if (IsSafeLocalReturnUrl(returnUrl) && IsReturnUrlAllowedForRole(returnUrl, role))
            {
                Session.Remove("ReturnUrl");
                Response.Redirect(returnUrl, false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            RedirectByRole(role);
        }

        // Accept only local return paths and prevent learners from entering administrator routes.
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

        private void ShowMessage(string message, string type)
        {
            lblMessage.Text = message;
            pnlMessage.CssClass = "form-message " + type;
            pnlMessage.Visible = true;
        }

        // Supports the short-lived registration session before its database-backed sign-in completes.
        private bool IsRegisteredLearner(string email, string password)
        {
            if (Session["RegisteredEmail"] == null ||
                Session["RegisteredPasswordSalt"] == null ||
                Session["RegisteredPasswordHash"] == null)
            {
                return false;
            }

            if (!email.Equals(Session["RegisteredEmail"].ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            byte[] salt = Convert.FromBase64String(Session["RegisteredPasswordSalt"].ToString());
            byte[] expectedHash = Convert.FromBase64String(Session["RegisteredPasswordHash"].ToString());

            using (Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(password, salt, 100000))
            {
                byte[] actualHash = deriveBytes.GetBytes(32);
                return SlowEquals(actualHash, expectedHash);
            }
        }

        private static bool SlowEquals(byte[] first, byte[] second)
        {
            uint difference = (uint)first.Length ^ (uint)second.Length;
            int length = Math.Min(first.Length, second.Length);

            for (int index = 0; index < length; index++)
            {
                difference |= (uint)(first[index] ^ second[index]);
            }

            return difference == 0;
        }
    }
}
