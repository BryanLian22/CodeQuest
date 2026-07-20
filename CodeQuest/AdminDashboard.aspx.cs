using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using CodeQuest.Data.Repositories;
using CodeQuest.Models;

namespace CodeQuest
{
    public partial class AdminDashboard : System.Web.UI.Page
    {
        protected override void OnPreInit(EventArgs e)
        {
            Response.ContentEncoding = Encoding.UTF8;
            Response.Charset = "utf-8";
            base.OnPreInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!string.Equals(Convert.ToString(Session["UserRole"]), "Admin", StringComparison.OrdinalIgnoreCase))
            {
                Session["ReturnUrl"] = Request.RawUrl;
                Response.Redirect("Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadOverview();
            }
        }

        private void LoadOverview()
        {
            lblAdminName.Text = Server.HtmlEncode(Convert.ToString(Session["DisplayName"] ?? "Administrator"));

            try
            {
                AdminContentRepository repository = new AdminContentRepository();
                AdminContentSummary summary = repository.GetSummary();
                lblCourses.Text = summary.Courses.ToString();
                lblModules.Text = summary.Modules.ToString();
                lblChapters.Text = summary.Chapters.ToString();
                lblTutorials.Text = summary.Tutorials.ToString();
                lblExercises.Text = summary.Exercises.ToString();
                lblQuizzes.Text = summary.Quizzes.ToString();

                IList<AdminCourseRecord> courses = repository.GetCourses();
                rptCourses.DataSource = courses;
                rptCourses.DataBind();
                pnlEmpty.Visible = courses.Count == 0;
            }
            catch (ConfigurationErrorsException)
            {
                ShowMessage("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowMessage("The admin overview could not connect to CodeQuestDB.");
            }
        }

        private void ShowMessage(string message)
        {
            lblMessage.Text = Server.HtmlEncode(message);
            pnlMessage.Visible = true;
        }
    }
}
