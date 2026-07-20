using System;

namespace CodeQuest
{
    public partial class Contact : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string role = Convert.ToString(Session["UserRole"]);
            string destination;
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                destination = "Features/Admin/Support.aspx";
            }
            else if (string.Equals(role, "Learner", StringComparison.OrdinalIgnoreCase))
            {
                destination = "Features/Support/Tickets.aspx";
            }
            else
            {
                Session["ReturnUrl"] = Request.RawUrl;
                destination = "Login.aspx";
            }

            Response.Redirect(destination, false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
