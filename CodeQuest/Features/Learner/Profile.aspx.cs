// Purpose: Loads and validates learner profile updates without exposing authentication secrets.
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using CodeQuest.Data;
using CodeQuest.Data.Repositories;
using CodeQuest.Models;

namespace CodeQuest.Features.Learner
{
    public partial class Profile : System.Web.UI.Page
    {
        private static readonly Regex UsernamePattern = new Regex(@"^[A-Za-z0-9_]{3,30}$", RegexOptions.Compiled);
        private static readonly Regex PasswordPattern = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,100}$", RegexOptions.Compiled);

        protected override void OnPreInit(EventArgs e)
        {
            Response.ContentEncoding = Encoding.UTF8;
            Response.Charset = "utf-8";
            base.OnPreInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!string.Equals(Convert.ToString(Session["UserRole"]), "Learner", StringComparison.OrdinalIgnoreCase))
            {
                Session["ReturnUrl"] = Request.RawUrl;
                Response.Redirect("../../Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadProfile();
            }
        }

        protected void btnSaveProfile_Click(object sender, EventArgs e)
        {
            ResetMessages();
            int userID;
            if (!TryGetUserID(out userID))
            {
                ShowError("This session is not linked to a database learner. Sign out and use your registered account.");
                return;
            }

            string username = (txtUsername.Text ?? string.Empty).Trim();
            string bio = (txtBio.Text ?? string.Empty).Trim();
            if (!UsernamePattern.IsMatch(username))
            {
                ShowError("Use 3–30 letters, numbers or underscores for the username.");
                return;
            }

            if (bio.Length > 1000)
            {
                ShowError("Keep the biography within 1,000 characters.");
                return;
            }

            try
            {
                if (!new UserRepository().UpdateProfile(userID, username, bio))
                {
                    ShowError("Your account could not be found.");
                    return;
                }

                Session["DisplayName"] = username;
                ShowSuccess("Your profile has been updated.");
                LoadProfileFields(userID);
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException ex)
            {
                ShowError(ex.Number == 2601 || ex.Number == 2627
                    ? "That username is already used by another account."
                    : "Your profile could not be saved to CodeQuestDB.");
            }
        }

        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            ResetMessages();
            int userID;
            if (!TryGetUserID(out userID))
            {
                ShowError("This session is not linked to a database learner. Sign out and use your registered account.");
                return;
            }

            if (string.IsNullOrEmpty(txtCurrentPassword.Text))
            {
                ShowError("Enter your current password.");
                return;
            }

            if (!PasswordPattern.IsMatch(txtNewPassword.Text ?? string.Empty))
            {
                ShowError("The new password needs at least 8 characters, including uppercase, lowercase, a number and a symbol.");
                return;
            }

            if (!string.Equals(txtNewPassword.Text, txtConfirmPassword.Text, StringComparison.Ordinal))
            {
                ShowError("The new password and confirmation do not match.");
                return;
            }

            try
            {
                UserRepository repository = new UserRepository();
                UserRecord user = repository.FindByID(userID);
                if (user == null || !PasswordHasher.Verify(txtCurrentPassword.Text, user.PasswordHash))
                {
                    ShowError("The current password is incorrect.");
                    return;
                }

                if (PasswordHasher.Verify(txtNewPassword.Text, user.PasswordHash))
                {
                    ShowError("Choose a new password that is different from the current password.");
                    return;
                }

                if (!repository.UpdatePassword(userID, PasswordHasher.Hash(txtNewPassword.Text)))
                {
                    ShowError("Your account could not be found.");
                    return;
                }

                txtCurrentPassword.Text = string.Empty;
                txtNewPassword.Text = string.Empty;
                txtConfirmPassword.Text = string.Empty;
                ShowSuccess("Your password has been changed. Use the new password the next time you sign in.");
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowError("Your password could not be saved to CodeQuestDB.");
            }
        }

        private void LoadProfile()
        {
            ResetMessages();
            int userID;
            if (!TryGetUserID(out userID))
            {
                ShowError("This demo session has no dbo.User profile. Register or sign in with a database learner account.");
                return;
            }

            try
            {
                LoadProfileFields(userID);
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowError("The profile could not connect to CodeQuestDB.");
            }
        }

        private void LoadProfileFields(int userID)
        {
            UserRecord user = new UserRepository().FindByID(userID);
            if (user == null || !string.Equals(user.Role, "Learner", StringComparison.OrdinalIgnoreCase))
            {
                ShowError("The learner account could not be found.");
                return;
            }

            lblUserID.Text = user.UserID.ToString();
            txtUsername.Text = user.Username;
            lblEmail.Text = Server.HtmlEncode(user.Email);
            txtBio.Text = user.Bio ?? string.Empty;
            lblRole.Text = Server.HtmlEncode(user.Role);
            lblPlan.Text = Server.HtmlEncode(user.Plan);
            lblGoogleStatus.Text = string.IsNullOrWhiteSpace(user.GoogleID) ? "Not connected" : "Connected";
            Session["DisplayName"] = user.Username;
            Session["UserEmail"] = user.Email;
            Session["UserPlan"] = user.Plan;
        }

        private bool TryGetUserID(out int userID)
        {
            return int.TryParse(Convert.ToString(Session["UserID"]), out userID) && userID > 0;
        }

        private void ResetMessages()
        {
            pnlSuccess.Visible = false;
            pnlError.Visible = false;
        }

        private void ShowSuccess(string message)
        {
            lblSuccess.Text = Server.HtmlEncode(message);
            pnlSuccess.Visible = true;
            pnlError.Visible = false;
        }

        private void ShowError(string message)
        {
            lblError.Text = Server.HtmlEncode(message);
            pnlError.Visible = true;
            pnlSuccess.Visible = false;
        }
    }
}
