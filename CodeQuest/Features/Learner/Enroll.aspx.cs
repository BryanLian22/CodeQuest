// Purpose: Validates learner and plan access before creating an idempotent course enrolment.
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using CodeQuest.Data.Repositories;
using CodeQuest.Models;

namespace CodeQuest.Features.Learner
{
    public partial class Enroll : System.Web.UI.Page
    {
        private int CourseID
        {
            get
            {
                int courseID;
                return int.TryParse(Request.QueryString["courseId"], out courseID) ? courseID : 0;
            }
        }

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

            if (!IsPostBack)
            {
                LoadCourse();
            }
        }

        private void LoadCourse()
        {
            if (CourseID <= 0)
            {
                ShowError("The course link is missing a valid course ID.");
                return;
            }

            if (!string.Equals(Convert.ToString(Session["UserRole"]), "Learner", StringComparison.OrdinalIgnoreCase))
            {
                ShowError("Only learner accounts can enrol in a course. Return to your dashboard to manage the catalogue.");
                return;
            }

            try
            {
                CourseRecord course = new CourseRepository().GetByID(CourseID);
                if (course == null)
                {
                    ShowError("That course could not be found. Return to the catalogue and choose another course.");
                    return;
                }

                pnlCourse.Visible = true;
                lblCourseID.Text = course.CourseID.ToString();
                lblTitle.Text = Server.HtmlEncode(course.Title);
                lblDescription.Text = Server.HtmlEncode(string.IsNullOrWhiteSpace(course.Description)
                    ? "Build practical web development skills with guided lessons."
                    : course.Description);
                lblDifficulty.Text = Server.HtmlEncode(course.Difficulty);
                lblPlan.Text = Server.HtmlEncode(Convert.ToString(Session["UserPlan"] ?? "Basic"));

                int? userID = GetDatabaseUserID();
                if (!userID.HasValue)
                {
                    pnlPlanAllowed.Visible = false;
                    pnlLocked.Visible = true;
                    lblLocked.Text = "This demo sign-in is not linked to a database user. Register a real account to enrol and save progress.";
                    return;
                }

                if (new EnrollmentRepository().IsEnrolled(userID.Value, course.CourseID))
                {
                    pnlPlanAllowed.Visible = false;
                    pnlAlreadyEnrolled.Visible = true;
                    return;
                }

                if (!CanAccess(course.Difficulty, Convert.ToString(Session["UserPlan"] ?? "Basic")))
                {
                    pnlPlanAllowed.Visible = false;
                    pnlLocked.Visible = true;
                    lblLocked.Text = "Intermediate and Advanced courses require the Premium plan. Upgrade to continue learning this course.";
                }
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowError("The course could not be loaded from CodeQuestDB.");
            }
        }

        protected void btnEnroll_Click(object sender, EventArgs e)
        {
            int? userID = GetDatabaseUserID();
            if (!userID.HasValue)
            {
                ShowError("Register and sign in with a database account before enrolling.");
                return;
            }

            try
            {
                CourseRecord course = new CourseRepository().GetByID(CourseID);
                if (course == null)
                {
                    ShowError("That course could not be found.");
                    return;
                }

                string plan = Convert.ToString(Session["UserPlan"] ?? "Basic");
                if (!CanAccess(course.Difficulty, plan))
                {
                    pnlPlanAllowed.Visible = false;
                    pnlLocked.Visible = true;
                    lblLocked.Text = "Intermediate and Advanced courses require the Premium plan. Upgrade to continue learning this course.";
                    return;
                }

                EnrollmentRepository repository = new EnrollmentRepository();
                if (repository.IsEnrolled(userID.Value, course.CourseID))
                {
                    Session["DashboardMessage"] = "You are already enrolled in " + course.Title + ".";
                }
                else
                {
                    repository.CreateEnrollment(userID.Value, course.CourseID);
                    Session["DashboardMessage"] = "You are now enrolled in " + course.Title + ".";
                }

                Response.Redirect("../../LearnerDashboard.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException exception)
            {
                if (exception.Number == 2601 || exception.Number == 2627)
                {
                    Session["DashboardMessage"] = "You are already enrolled in this course.";
                    Response.Redirect("../../LearnerDashboard.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    ShowError("The enrolment could not be saved. Please try again.");
                }
            }
        }

        private int? GetDatabaseUserID()
        {
            if (Session["UserID"] == null || Session["UserID"] == DBNull.Value)
            {
                return null;
            }

            int userID;
            return int.TryParse(Session["UserID"].ToString(), out userID) && userID > 0
                ? (int?)userID
                : null;
        }

        private static bool CanAccess(string difficulty, string plan)
        {
            return string.Equals(plan, "Premium", StringComparison.OrdinalIgnoreCase)
                || string.Equals(difficulty, "Beginner", StringComparison.OrdinalIgnoreCase);
        }

        private void ShowError(string message)
        {
            lblError.Text = Server.HtmlEncode(message);
            pnlError.Visible = true;
        }
    }
}
