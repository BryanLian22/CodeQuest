using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using CodeQuest.Data.Repositories;
using CodeQuest.Models;

namespace CodeQuest.Features.Learner
{
    public partial class Course : System.Web.UI.Page
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

            bool isAdmin = string.Equals(Convert.ToString(Session["UserRole"]), "Admin", StringComparison.OrdinalIgnoreCase);
            phLearnerNavigation.Visible = !isAdmin;
            phAdminNavigation.Visible = isAdmin;
            phLearnerActions.Visible = !isAdmin;
            phAdminActions.Visible = isAdmin;
            pnlAdminPreview.Visible = isAdmin;
            if (isAdmin)
            {
                lnkBack.NavigateUrl = "../Public/Courses.aspx";
                lnkBack.Text = "&larr; Back to course previews";
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

            bool isAdmin = string.Equals(Convert.ToString(Session["UserRole"]), "Admin", StringComparison.OrdinalIgnoreCase);
            bool isLearner = string.Equals(Convert.ToString(Session["UserRole"]), "Learner", StringComparison.OrdinalIgnoreCase);
            if (!isLearner && !isAdmin)
            {
                ShowError("Only learner or administrator accounts can open course content.");
                return;
            }

            int userID = 0;
            if (isLearner && (!int.TryParse(Convert.ToString(Session["UserID"]), out userID) || userID <= 0))
            {
                ShowError("This sign-in is not linked to a database learner. Register a real account to open course content.");
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

                pnlCourse.Visible = true;
                lblCourseID.Text = course.CourseID.ToString();
                lblTitle.Text = Server.HtmlEncode(course.Title);
                lblDescription.Text = Server.HtmlEncode(string.IsNullOrWhiteSpace(course.Description)
                    ? "Build practical web development skills with guided lessons."
                    : course.Description);
                lblDifficulty.Text = Server.HtmlEncode(course.Difficulty);

                if (isLearner && !new EnrollmentRepository().IsEnrolled(userID, course.CourseID))
                {
                    pnlNotEnrolled.Visible = true;
                    lnkEnroll.NavigateUrl = "Enroll.aspx?courseId=" + course.CourseID;
                    return;
                }

                CourseContentRepository contentRepository = new CourseContentRepository();
                IList<ModuleRecord> modules = isAdmin
                    ? contentRepository.GetModulesForPreview(course.CourseID)
                    : contentRepository.GetPublishedModules(course.CourseID, userID);
                if (isLearner)
                {
                    new EnrollmentRepository().CompleteCourseIfReady(userID, course.CourseID);
                }
                pnlContent.Visible = true;
                rptModules.DataSource = modules;
                rptModules.DataBind();
                pnlNoContent.Visible = modules.Count == 0;
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowError("The course content could not be loaded from CodeQuestDB.");
            }
        }

        private void ShowError(string message)
        {
            lblError.Text = Server.HtmlEncode(message);
            pnlError.Visible = true;
        }
    }
}
