using System;
using System.Security.Cryptography;
using System.Text;

namespace CodeQuest
{
    public partial class Login : System.Web.UI.Page
    {
        // Temporary accounts for interface testing only.
        // Replace this method with a SQL Server lookup and password-hash verification
        // when the User table is implemented.
        private const string LearnerEmail = "learner@codequest.io";
        private const string LearnerPassword = "Learner123!";
        private const string AdminEmail = "admin@codequest.io";
        private const string AdminPassword = "Admin123!";

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

            if (Session["UserRole"] != null)
            {
                RedirectByRole(Session["UserRole"].ToString());
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

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            if (email.Equals(LearnerEmail, StringComparison.OrdinalIgnoreCase) &&
                password == LearnerPassword)
            {
                SignIn("Alex", LearnerEmail, "Learner");
                RedirectByRole("Learner");
                return;
            }

            if (email.Equals(AdminEmail, StringComparison.OrdinalIgnoreCase) &&
                password == AdminPassword)
            {
                SignIn("Administrator", AdminEmail, "Admin");
                RedirectByRole("Admin");
                return;
            }

            if (IsRegisteredLearner(email, password))
            {
                string username = Session["RegisteredUsername"] == null
                    ? "Learner"
                    : Session["RegisteredUsername"].ToString();

                SignIn(username, email, "Learner");
                RedirectByRole("Learner");
                return;
            }

            ShowMessage("The email address or password is incorrect.", "error");
        }

        protected void btnGoogle_Click(object sender, EventArgs e)
        {
            ShowMessage("Google sign-in will be connected after the basic login system is complete.", "info");
        }

        private void SignIn(string displayName, string email, string role)
        {
            Session["DisplayName"] = displayName;
            Session["UserEmail"] = email;
            Session["UserRole"] = role;

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
            string destination = role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                ? "AdminDashboard.aspx"
                : "LearnerDashboard.aspx";

            Response.Redirect(destination, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void ShowMessage(string message, string type)
        {
            lblMessage.Text = message;
            pnlMessage.CssClass = "form-message " + type;
            pnlMessage.Visible = true;
        }

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
