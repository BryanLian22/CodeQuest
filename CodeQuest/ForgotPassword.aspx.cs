// Purpose: Issues time-limited reset tokens and sends neutral, privacy-safe reset responses.
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using CodeQuest.Data.Repositories;
using CodeQuest.Models;

namespace CodeQuest
{
    public partial class ForgotPassword : System.Web.UI.Page
    {
        private static readonly Regex EmailPattern = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

        protected override void OnPreInit(EventArgs e)
        {
            Response.ContentEncoding = Encoding.UTF8;
            Response.Charset = "utf-8";
            base.OnPreInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Session["UserRole"] != null)
            {
                string destination = string.Equals(Convert.ToString(Session["UserRole"]), "Admin", StringComparison.OrdinalIgnoreCase)
                    ? "AdminDashboard.aspx"
                    : "LearnerDashboard.aspx";
                Response.Redirect(destination, false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        protected void btnRequestReset_Click(object sender, EventArgs e)
        {
            pnlMessage.Visible = false;
            pnlLocalReset.Visible = false;

            string email = (txtEmail.Text ?? string.Empty).Trim();
            if (!EmailPattern.IsMatch(email))
            {
                ShowMessage("Enter a valid email address.", "error");
                return;
            }

            try
            {
                PasswordResetIssue issue = new PasswordResetRepository().Create(email, TimeSpan.FromMinutes(30));
                string message = "If an account uses that email address, a password-reset link is ready.";
                if (issue != null)
                {
                    string resetUrl = BuildResetUrl(issue.RawToken);
                    bool emailSent = TrySendResetEmail(issue.Email, resetUrl, issue.ExpiresAt);
                    if (emailSent)
                    {
                        message += " Check your inbox for the one-time link.";
                    }
                    else
                    {
                        message += " SMTP email delivery is not configured, so use the local development link below.";
                        lnkLocalReset.NavigateUrl = resetUrl;
                        lblExpiry.Text = issue.ExpiresAt.ToLocalTime().ToString("dd MMM yyyy, HH:mm");
                        pnlLocalReset.Visible = true;
                    }
                }

                ShowMessage(message, "success");
            }
            catch (ConfigurationErrorsException)
            {
                ShowMessage("The database connection is not configured. Add CodeQuestDb to Web.config.", "error");
            }
            catch (SqlException)
            {
                ShowMessage("The reset request could not be saved to CodeQuestDB.", "error");
            }
        }

        private string BuildResetUrl(string rawToken)
        {
            string path = ResolveUrl("~/ResetPassword.aspx?token=" + HttpUtility.UrlEncode(rawToken));
            return Request.Url.GetLeftPart(UriPartial.Authority) + path;
        }

        private bool TrySendResetEmail(string email, string resetUrl, DateTime expiresAt)
        {
            string host = ConfigurationManager.AppSettings["CodeQuestSmtpHost"];
            string from = ConfigurationManager.AppSettings["CodeQuestResetFromEmail"];
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
            {
                return false;
            }

            try
            {
                int port;
                int.TryParse(ConfigurationManager.AppSettings["CodeQuestSmtpPort"], out port);
                if (port <= 0) port = 25;

                using (MailMessage message = new MailMessage(from, email))
                using (SmtpClient client = new SmtpClient(host, port))
                {
                    message.Subject = "Reset your CodeQuest password";
                    message.Body = "Use this one-time CodeQuest password reset link:\r\n\r\n"
                        + resetUrl + "\r\n\r\nIt expires at " + expiresAt.ToUniversalTime().ToString("u") + ".";
                    client.EnableSsl = string.Equals(ConfigurationManager.AppSettings["CodeQuestSmtpSsl"], "true", StringComparison.OrdinalIgnoreCase);

                    string username = ConfigurationManager.AppSettings["CodeQuestSmtpUsername"];
                    string password = ConfigurationManager.AppSettings["CodeQuestSmtpPassword"];
                    if (!string.IsNullOrWhiteSpace(username))
                    {
                        client.Credentials = new NetworkCredential(username, password ?? string.Empty);
                    }

                    client.Send(message);
                }

                return true;
            }
            catch (SmtpException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
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
