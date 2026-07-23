// Purpose: Builds the landing-page navigation and call-to-action state for guests, learners and administrators.
using System;
using System.Text;

namespace CodeQuest
{
    public partial class Guest : System.Web.UI.Page
    {
        protected override void OnPreInit(EventArgs e)
        {
            Response.ContentEncoding = Encoding.UTF8;
            Response.Charset = "utf-8";
            base.OnPreInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ConfigureHeader();
        }

        private void ConfigureHeader()
        {
            string role = Convert.ToString(Session["UserRole"]);
            bool isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            bool isLearner = string.Equals(role, "Learner", StringComparison.OrdinalIgnoreCase);

            phGuestHeaderActions.Visible = !isAdmin && !isLearner;
            phLearnerHeaderActions.Visible = isLearner;
            phAdminHeaderActions.Visible = isAdmin;
        }
    }
}
