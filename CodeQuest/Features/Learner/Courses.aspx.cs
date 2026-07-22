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

        private void ShowError(string message)
        {
            lblError.Text = Server.HtmlEncode(message);
            pnlError.Visible = true;
            pnlEmpty.Visible = false;
        }
    }
}
