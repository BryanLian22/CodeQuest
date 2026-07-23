// Purpose: Validates new learner details, hashes passwords and starts email or Google registration.
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI.WebControls;
using CodeQuest.Data;
using CodeQuest.Data.Repositories;

namespace CodeQuest
{
    public partial class Register : System.Web.UI.Page
    {
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

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            Page.Validate("RegisterValidation");

            if (!Page.IsValid)
            {
                return;
            }

            try
            {
                string passwordHash = PasswordHasher.Hash(txtPassword.Text);
                UserRepository repository = new UserRepository();
                repository.CreateLearner(txtUsername.Text, passwordHash, txtEmail.Text);

                Session["LoginMessage"] = "Account created successfully. You can now log in.";
                Session["RegisteredEmail"] = txtEmail.Text.Trim();

                Response.Redirect("Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    ShowMessage("That username or email address is already registered.", "error");
                }
                else
                {
                    ShowMessage("The database could not be reached. Confirm that CodeQuestDB is running in LocalDB.", "error");
                }
            }
        }

        protected void btnGoogleRegister_Click(object sender, EventArgs e)
        {
            if (!GoogleOAuthClient.IsConfigured)
            {
                ShowMessage("Google sign-in is not configured yet. Add CodeQuestGoogleClientId and CodeQuestGoogleClientSecret to Web.config.", "info");
                return;
            }

            string state = GoogleOAuthClient.CreateState();
            Session[Login.GoogleOAuthStateSessionKey] = state;
            string redirectUri = GoogleOAuthClient.GetRedirectUri(Request);
            string authorizationUrl = GoogleOAuthClient.BuildAuthorizationUrl(state, redirectUri);

            Response.Redirect(authorizationUrl, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void cvTerms_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = chkTerms.Checked;
        }

        private void ShowMessage(string message, string type)
        {
            lblMessage.Text = message;
            pnlMessage.CssClass = "form-message " + type;
            pnlMessage.Visible = true;
        }
    }
}
