// Purpose: Validates one-time tokens and replaces passwords with salted PBKDF2 hashes.
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using CodeQuest.Data;
using CodeQuest.Data.Repositories;
using CodeQuest.Models;

namespace CodeQuest
{
    public partial class ResetPassword : System.Web.UI.Page
    {
        private static readonly Regex PasswordPattern = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,100}$", RegexOptions.Compiled);

        private string RawToken
        {
            get { return Convert.ToString(Request.QueryString["token"] ?? string.Empty); }
        }

        protected override void OnPreInit(EventArgs e)
        {
            Response.ContentEncoding = Encoding.UTF8;
            Response.Charset = "utf-8";
            base.OnPreInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(RawToken))
            {
                ShowMessage("This password-reset link is missing or invalid.", "error");
                return;
            }

            try
            {
                PasswordResetTarget target = new PasswordResetRepository().FindValid(RawToken);
                if (target == null)
                {
                    ShowMessage("This password-reset link has expired or has already been used. Request a new link.", "error");
                    return;
                }

                pnlResetForm.Visible = true;
                ShowMessage("The link is valid for " + target.Email + ". Choose a new password below.", "success");
            }
            catch (ConfigurationErrorsException)
            {
                ShowMessage("The database connection is not configured. Add CodeQuestDb to Web.config.", "error");
            }
            catch (SqlException)
            {
                ShowMessage("The reset link could not be checked in CodeQuestDB.", "error");
            }
        }

        protected void btnResetPassword_Click(object sender, EventArgs e)
        {
            if (!PasswordPattern.IsMatch(txtNewPassword.Text ?? string.Empty))
            {
                ShowMessage("The new password needs at least 8 characters, including uppercase, lowercase, a number and a symbol.", "error");
                return;
            }

            if (!string.Equals(txtNewPassword.Text, txtConfirmPassword.Text, StringComparison.Ordinal))
            {
                ShowMessage("The new password and confirmation do not match.", "error");
                return;
            }

            if (string.IsNullOrWhiteSpace(RawToken))
            {
                ShowMessage("This password-reset link is missing or invalid.", "error");
                return;
            }

            try
            {
                bool saved = new PasswordResetRepository().ResetPassword(
                    RawToken,
                    PasswordHasher.Hash(txtNewPassword.Text));

                if (!saved)
                {
                    ShowMessage("This password-reset link has expired or has already been used. Request a new link.", "error");
                    pnlResetForm.Visible = false;
                    return;
                }

                Session["LoginMessage"] = "Your password was reset successfully. You can now log in with the new password.";
                Response.Redirect("Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (ConfigurationErrorsException)
            {
                ShowMessage("The database connection is not configured. Add CodeQuestDb to Web.config.", "error");
            }
            catch (SqlException)
            {
                ShowMessage("The new password could not be saved to CodeQuestDB.", "error");
            }
        }

        private void ShowMessage(string message, string type)
        {
            lblMessage.Text = Server.HtmlEncode(message);
            pnlMessage.CssClass = "form-message " + type;
            pnlMessage.Visible = true;
        }
    }
}
