// Purpose: Loads public courses and produces role-aware catalogue actions and navigation.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using CodeQuest.Data.Repositories;
using CodeQuest.Models;

namespace CodeQuest.Features.Public
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
            if (!IsPostBack)
            {
                LoadCourses();
            }
        }

        private void LoadCourses()
        {
            bool isAdmin = string.Equals(Convert.ToString(Session["UserRole"]), "Admin", StringComparison.OrdinalIgnoreCase);
            bool isLearner = string.Equals(Convert.ToString(Session["UserRole"]), "Learner", StringComparison.OrdinalIgnoreCase);
            phPublicNavigation.Visible = !isAdmin;
            phAdminNavigation.Visible = isAdmin;
            phPublicActions.Visible = !isAdmin;
            phAdminActions.Visible = isAdmin;
            pnlAdminPreview.Visible = isAdmin;
            if (isLearner)
            {
                lnkPrimaryHeader.NavigateUrl = "../Learner/Courses.aspx";
                lnkPrimaryHeader.Text = "Learner courses";
                lnkSecondaryHeader.NavigateUrl = "../../Login.aspx?logout=1";
                lnkSecondaryHeader.Text = "Sign out";
            }

            try
            {
                IList<CourseRecord> courses = new CourseRepository().GetAllCourses();
                ISet<int> enrolledCourseIDs = new HashSet<int>();
                int userID;
                bool signedIn = int.TryParse(Convert.ToString(Session["UserID"]), out userID) && userID > 0;

                if (signedIn && string.Equals(Convert.ToString(Session["UserRole"]), "Learner", StringComparison.OrdinalIgnoreCase))
                {
                    enrolledCourseIDs = new EnrollmentRepository().GetCourseIDsForUser(userID);
                }

                foreach (CourseRecord course in courses)
                {
                    if (isAdmin)
                    {
                        course.ActionText = "Preview course";
                        course.ActionUrl = "../Learner/Course.aspx?courseId=" + course.CourseID;
                        continue;
                    }

                    course.IsEnrolled = enrolledCourseIDs.Contains(course.CourseID);
                    course.ActionText = course.IsEnrolled
                        ? "Continue course"
                        : signedIn ? "Enrol now" : "Log in to enrol";
                    course.ActionUrl = course.IsEnrolled
                        ? "../Learner/Course.aspx?courseId=" + course.CourseID
                        : "../Learner/Enroll.aspx?courseId=" + course.CourseID;
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
                ShowError("The course catalogue could not connect to CodeQuestDB.");
            }
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            pnlError.Visible = true;
            pnlEmpty.Visible = false;
        }
    }
}
