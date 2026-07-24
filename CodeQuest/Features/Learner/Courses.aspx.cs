// Purpose: Loads the learner catalogue and selects enrol, continue or completed actions for each course.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using CodeQuest.Data.Repositories;
using CodeQuest.Models;

namespace CodeQuest.Features.Learner
{
    public partial class Courses : System.Web.UI.Page
    {
        private string userPlan = "Basic";

        protected override void OnPreInit(EventArgs e)
        {
            Response.ContentEncoding = Encoding.UTF8;
            Response.Charset = "utf-8";
            base.OnPreInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserRole"] == null)
            {
                Session["ReturnUrl"] = Request.RawUrl;
                Response.Redirect("../../Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!string.Equals(Convert.ToString(Session["UserRole"]), "Learner", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("../../AdminDashboard.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadCourses();
            }
        }

        private void LoadCourses()
        {
            int userID;
            if (!int.TryParse(Convert.ToString(Session["UserID"]), out userID) || userID <= 0)
            {
                ShowError("Your learner session is not linked to a database account. Sign in again.");
                return;
            }

            try
            {
                UserRecord account = new UserRepository().FindByID(userID);
                if (account != null)
                {
                    // Refresh the plan so administrator changes immediately affect
                    // both course access and the catalogue's restricted styling.
                    userPlan = string.IsNullOrWhiteSpace(account.Plan) ? "Basic" : account.Plan;
                    Session["UserPlan"] = userPlan;
                }
                else
                {
                    userPlan = Convert.ToString(Session["UserPlan"] ?? "Basic");
                }

                IList<CourseRecord> courses = new CourseRepository().GetAllCourses();
                IList<EnrollmentCourseRecord> enrollments = new EnrollmentRepository().GetForUser(userID);
                Dictionary<int, string> statusByCourseID = new Dictionary<int, string>();

                foreach (EnrollmentCourseRecord enrollment in enrollments)
                {
                    statusByCourseID[enrollment.CourseID] = enrollment.Status;
                }

                foreach (CourseRecord course in courses)
                {
                    string status;
                    if (statusByCourseID.TryGetValue(course.CourseID, out status))
                    {
                        course.IsEnrolled = true;
                        course.ActionText = string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase)
                            ? "Review course"
                            : "Continue course";
                        course.ActionUrl = "Course.aspx?courseId=" + course.CourseID;
                    }
                    else
                    {
                        course.IsEnrolled = false;
                        course.ActionText = "Enrol now";
                        course.ActionUrl = "Enroll.aspx?courseId=" + course.CourseID;
                    }
                }

                rptCourses.DataSource = courses;
                rptCourses.DataBind();
                pnlEmpty.Visible = courses.Count == 0;
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowError("The learner course catalogue could not connect to CodeQuestDB.");
            }
        }

        /// <summary>
        /// Uses the same access rule as the enrolment page: Basic learners can
        /// access Beginner courses, while other difficulties require Premium.
        /// </summary>
        protected string GetDifficultyCss(object difficultyValue)
        {
            string difficulty = Convert.ToString(difficultyValue);
            bool canAccess = string.Equals(userPlan, "Premium", StringComparison.OrdinalIgnoreCase)
                || string.Equals(difficulty, "Beginner", StringComparison.OrdinalIgnoreCase);

            return canAccess
                ? "catalogue-level"
                : "catalogue-level catalogue-level-restricted";
        }

        private void ShowError(string message)
        {
            lblError.Text = Server.HtmlEncode(message);
            pnlError.Visible = true;
            pnlEmpty.Visible = false;
        }
    }
}
