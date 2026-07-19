using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI.WebControls;

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
                Response.Redirect("LearnerDashboard.aspx", false);
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

            byte[] salt;
            byte[] passwordHash = HashPassword(txtPassword.Text, out salt);

            // Temporary prototype storage. Replace these Session values with an
            // INSERT into the SQL Server User table when the database is added.
            Session["RegisteredUsername"] = txtUsername.Text.Trim();
            Session["RegisteredEmail"] = txtEmail.Text.Trim();
            Session["RegisteredPasswordSalt"] = Convert.ToBase64String(salt);
            Session["RegisteredPasswordHash"] = Convert.ToBase64String(passwordHash);
            Session["LoginMessage"] = "Account created successfully. You can now log in.";

            Response.Redirect("Login.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnGoogleRegister_Click(object sender, EventArgs e)
        {
            ShowMessage("Google registration will be connected after the basic account system is complete.", "info");
        }

        protected void cvTerms_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = chkTerms.Checked;
        }

        private static byte[] HashPassword(string password, out byte[] salt)
        {
            salt = new byte[32];

            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
            }

            using (Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(password, salt, 100000))
            {
                return deriveBytes.GetBytes(32);
            }
        }

        private void ShowMessage(string message, string type)
        {
            lblMessage.Text = message;
            pnlMessage.CssClass = "form-message " + type;
            pnlMessage.Visible = true;
        }
    }
}
