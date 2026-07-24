// Purpose: Aggregates learner enrolment, progress, streak, quiz and plan data for the dashboard.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using CodeQuest.Data.Repositories;
using CodeQuest.Models;

namespace CodeQuest
{
    public partial class LearnerDashboard : System.Web.UI.Page
    {
        protected override void OnPreInit(EventArgs e)
        {
            Response.ContentEncoding = Encoding.UTF8;
            Response.Charset = "utf-8";
            base.OnPreInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            string role = Convert.ToString(Session["UserRole"]);
            if (string.IsNullOrWhiteSpace(role))
            {
                Response.Redirect("Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!string.Equals(role, "Learner", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("AdminDashboard.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadDashboard();
            }
        }

        private void LoadDashboard()
        {
            lblDisplayName.Text = Server.HtmlEncode(Convert.ToString(Session["DisplayName"] ?? "Learner"));
            lblPlan.Text = Convert.ToString(Session["UserPlan"] ?? "Basic");

            if (Session["DashboardMessage"] != null)
            {
                ShowMessage(Session["DashboardMessage"].ToString());
                Session.Remove("DashboardMessage");
            }

            if (Session["UserID"] == null || Session["UserID"] == DBNull.Value)
            {
                ShowMessage("This demo session is not linked to a database user yet. Register a new account to load real enrollments.");
                pnlEmpty.Visible = true;
                return;
            }

            try
            {
                int userID = Convert.ToInt32(Session["UserID"]);
                UserRecord account = new UserRepository().FindByID(userID);
                if (account != null)
                {
                    // Refresh access from the database so an administrator's
                    // plan change is reflected without relying on stale session data.
                    Session["DisplayName"] = account.Username;
                    Session["UserPlan"] = account.Plan;
                    lblDisplayName.Text = Server.HtmlEncode(account.Username);
                    lblPlan.Text = Server.HtmlEncode(account.Plan);
                }

                IList<EnrollmentCourseRecord> enrollments = new EnrollmentRepository().GetForUser(userID);
                rptEnrollments.DataSource = enrollments;
                rptEnrollments.DataBind();
                lblCourseCount.Text = enrollments.Count.ToString();
                pnlEmpty.Visible = enrollments.Count == 0;

                try
                {
                    ProgressRepository progress = new ProgressRepository();
                    lblCompletedLessons.Text = progress.GetCompletedChapterCount(userID).ToString();
                    decimal? average = progress.GetQuizAverage(userID);
                    lblQuizAverage.Text = average.HasValue ? average.Value.ToString("0") + "%" : "--";
                }
                catch (SqlException)
                {
                    ShowMessage("Saved quiz progress is unavailable. Check the CodeQuestDb connection and automatic database setup.");
                }
            }
            catch (ConfigurationErrorsException)
            {
                ShowMessage("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowMessage("The learner dashboard could not connect to CodeQuestDB.");
            }
        }

        private void ShowMessage(string message)
        {
            lblMessage.Text = message;
            pnlMessage.Visible = true;
        }

        protected string GetEnrollmentStatusCss(object statusValue)
        {
            return string.Equals(Convert.ToString(statusValue), "Completed", StringComparison.OrdinalIgnoreCase)
                ? "enrollment-status completed"
                : "enrollment-status";
        }

        protected string GetEnrollmentAction(object statusValue)
        {
            return string.Equals(Convert.ToString(statusValue), "Completed", StringComparison.OrdinalIgnoreCase)
                ? "Review course &rarr;"
                : "Continue course &rarr;";
        }
    }
}
