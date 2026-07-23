// Purpose: Enforces learner access, renders chapter content and maintains view or quiz-based progress rules.
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using CodeQuest.Data.Repositories;
using CodeQuest.Models;

namespace CodeQuest.Features.Learner
{
    public partial class Chapter : System.Web.UI.Page
    {
        private int ChapterID
        {
            get
            {
                int chapterID;
                return int.TryParse(Request.QueryString["chapterId"], out chapterID) ? chapterID : 0;
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
            phLearnerBreadcrumb.Visible = !isAdmin;
            phAdminBreadcrumb.Visible = isAdmin;
            pnlAdminPreview.Visible = isAdmin;
            lnkAssistant.Visible = !isAdmin;

            if (!IsPostBack)
            {
                LoadChapter();
            }
        }

        private void LoadChapter()
        {
            if (ChapterID <= 0)
            {
                ShowError("The chapter link is missing a valid chapter ID.");
                return;
            }

            bool isAdmin = string.Equals(Convert.ToString(Session["UserRole"]), "Admin", StringComparison.OrdinalIgnoreCase);
            bool isLearner = string.Equals(Convert.ToString(Session["UserRole"]), "Learner", StringComparison.OrdinalIgnoreCase);
            if (!isLearner && !isAdmin)
            {
                ShowError("Only learner or administrator accounts can open chapter content.");
                return;
            }

            int userID = 0;
            if (isLearner && (!int.TryParse(Convert.ToString(Session["UserID"]), out userID) || userID <= 0))
            {
                ShowError("This sign-in is not linked to a database learner. Register a real account to save learning progress.");
                return;
            }

            try
            {
                ChapterLessonRecord lesson = new ChapterContentRepository().GetChapter(ChapterID, isAdmin);
                if (lesson == null)
                {
                    ShowError("That chapter could not be found or is not published.");
                    return;
                }

                if (isLearner && !new EnrollmentRepository().IsEnrolled(userID, lesson.CourseID))
                {
                    Response.Redirect("Course.aspx?courseId=" + lesson.CourseID, false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                bool hasQuiz = new QuizRepository().HasQuiz(lesson.ChapterID, isAdmin);
                bool chapterCompleted = false;
                bool courseCompleted = false;
                if (isLearner)
                {
                    ProgressRepository progress = new ProgressRepository();
                    if (!hasQuiz)
                    {
                        progress.MarkChapterCompleted(userID, lesson.ChapterID);
                    }

                    chapterCompleted = progress.IsChapterCompleted(userID, lesson.ChapterID);
                    courseCompleted = new EnrollmentRepository().CompleteCourseIfReady(userID, lesson.CourseID);
                    if (courseCompleted)
                    {
                        Session["DashboardMessage"] = "Congratulations! You completed " + lesson.CourseTitle + ".";
                    }
                }

                BindLesson(lesson, courseCompleted, hasQuiz, chapterCompleted, isAdmin);
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowError("The chapter content could not be loaded from CodeQuestDB.");
            }
        }

        private void BindLesson(
            ChapterLessonRecord lesson,
            bool courseCompleted,
            bool hasQuiz,
            bool chapterCompleted,
            bool isAdmin)
        {
            pnlChapter.Visible = true;
            lblChapterID.Text = lesson.ChapterID.ToString();
            lblTitle.Text = Server.HtmlEncode(lesson.ChapterTitle);
            lblDescription.Text = Server.HtmlEncode(string.IsNullOrWhiteSpace(lesson.ChapterDescription)
                ? "Work through this chapter and complete the practice question."
                : lesson.ChapterDescription);
            lnkBreadcrumbCourse.Text = Server.HtmlEncode(lesson.CourseTitle);
            lnkBreadcrumbCourse.NavigateUrl = "Course.aspx?courseId=" + lesson.CourseID;
            lnkBreadcrumbModule.Text = Server.HtmlEncode(lesson.ModuleTitle);
            lnkBreadcrumbModule.NavigateUrl = "Course.aspx?courseId=" + lesson.CourseID + "#module-" + lesson.ModuleID;
            lnkCourse.NavigateUrl = "Course.aspx?courseId=" + lesson.CourseID;
            lnkAssistant.NavigateUrl = "../AI/Assistant.aspx?chapterId=" + lesson.ChapterID;
            pnlQuizLink.Visible = false;
            if (isAdmin)
            {
                lblChapterNavigationNote.Text = "Preview complete. No learner progress was recorded.";
            }
            else if (hasQuiz && !chapterCompleted)
            {
                lblChapterNavigationNote.Text = "Pass the chapter quiz with 75% or higher to mark this chapter as done.";
            }
            else
            {
                lblChapterNavigationNote.Text = "This chapter is marked as done.";
            }

            if (!isAdmin && hasQuiz && !chapterCompleted)
            {
                lnkNextChapter.Text = "Take quiz to complete &rarr;";
                lnkNextChapter.NavigateUrl = "Quiz.aspx?chapterId=" + lesson.ChapterID;
            }
            else
            {
                int? nextChapterID = new ChapterContentRepository().GetNextChapterID(lesson.ChapterID, isAdmin);
                if (nextChapterID.HasValue)
                {
                    lnkNextChapter.Text = "Next chapter &rarr;";
                    lnkNextChapter.NavigateUrl = "Chapter.aspx?chapterId=" + nextChapterID.Value;
                }
                else
                {
                    lnkNextChapter.Text = courseCompleted ? "Back to completed course &rarr;" : "Back to course &rarr;";
                    lnkNextChapter.NavigateUrl = "Course.aspx?courseId=" + lesson.CourseID;
                }
            }

            if (lesson.TutorialID.HasValue)
            {
                pnlTutorial.Visible = true;
                lblTutorialTitle.Text = Server.HtmlEncode(lesson.TutorialTitle);
                string materials = string.IsNullOrWhiteSpace(lesson.Materials)
                    ? "Tutorial material is being prepared."
                    : lesson.Materials;
                litMaterials.Text = Server.HtmlEncode(materials.Replace("\\n", Environment.NewLine));
            }
            else
            {
                pnlNoTutorial.Visible = true;
            }

            if (lesson.Exercises.Count > 0)
            {
                ExerciseRecord exercise = lesson.Exercises[0];
                pnlExercise.Visible = true;
                lblExerciseQuestion.Text = Server.HtmlEncode(exercise.Question);
                ViewState["CorrectAnswer"] = exercise.CorrectAnswer;
            }

            if (hasQuiz)
            {
                pnlQuizLink.Visible = true;
                lnkQuiz.NavigateUrl = "Quiz.aspx?chapterId=" + lesson.ChapterID;
            }
        }

        protected void btnCheckAnswer_Click(object sender, EventArgs e)
        {
            string expected = Convert.ToString(ViewState["CorrectAnswer"]);
            string actual = txtAnswer.Text.Trim();
            lblExerciseResult.Visible = true;

            if (!string.IsNullOrWhiteSpace(expected) && string.Equals(actual, expected.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                lblExerciseResult.Text = "Correct. Nice work.";
                lblExerciseResult.CssClass = "exercise-result correct";
            }
            else
            {
                lblExerciseResult.Text = "Not quite yet. Review the tutorial and try again.";
                lblExerciseResult.CssClass = "exercise-result incorrect";
            }
        }

        private void ShowError(string message)
        {
            lblError.Text = Server.HtmlEncode(message);
            pnlError.Visible = true;
        }
    }
}
