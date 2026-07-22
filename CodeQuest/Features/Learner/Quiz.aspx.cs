using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI.WebControls;
using CodeQuest.Data.Repositories;
using CodeQuest.Models;

namespace CodeQuest.Features.Learner
{
    public partial class Quiz : System.Web.UI.Page
    {
        private int ChapterID
        {
            get
            {
                int chapterID;
                return int.TryParse(Request.QueryString["chapterId"], out chapterID) ? chapterID : 0;
            }
        }

        private bool IsRetake
        {
            get { return string.Equals(Request.QueryString["retake"], "1", StringComparison.Ordinal); }
        }

        private bool IsAdmin
        {
            get { return string.Equals(Convert.ToString(Session["UserRole"]), "Admin", StringComparison.OrdinalIgnoreCase); }
        }

        protected override void OnPreInit(EventArgs e)
        {
            Response.ContentEncoding = Encoding.UTF8;
            Response.Charset = "utf-8";
            base.OnPreInit(e);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Recreate the Repeater controls before postback data is applied so
            // selected radio answers are available to the submit event.
            if (IsPostBack && ChapterID > 0)
            {
                try
                {
                    BindQuestions(new QuizRepository().GetForChapter(ChapterID, IsAdmin));
                }
                catch (SqlException)
                {
                    // Page_Load will show the actionable database message.
                }
                catch (ConfigurationErrorsException)
                {
                    // Page_Load will show the actionable database message.
                }
            }
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

            phLearnerNavigation.Visible = !IsAdmin;
            phAdminNavigation.Visible = IsAdmin;
            phLearnerActions.Visible = !IsAdmin;
            phAdminActions.Visible = IsAdmin;
            phLearnerBreadcrumb.Visible = !IsAdmin;
            phAdminBreadcrumb.Visible = IsAdmin;
            pnlAdminPreview.Visible = IsAdmin;

            if (!IsPostBack)
            {
                LoadQuiz();
            }
        }

        private void LoadQuiz()
        {
            if (ChapterID <= 0)
            {
                ShowError("The quiz link is missing a valid chapter ID.");
                return;
            }

            bool isLearner = string.Equals(Convert.ToString(Session["UserRole"]), "Learner", StringComparison.OrdinalIgnoreCase);
            if (!isLearner && !IsAdmin)
            {
                ShowError("Only learner or administrator accounts can open chapter quizzes.");
                return;
            }

            int userID = 0;
            if (isLearner && (!int.TryParse(Convert.ToString(Session["UserID"]), out userID) || userID <= 0))
            {
                ShowError("This sign-in is not linked to a database learner. Register a real account to save quiz progress.");
                return;
            }

            try
            {
                ChapterLessonRecord lesson = new ChapterContentRepository().GetChapter(ChapterID, IsAdmin);
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

                IList<QuizQuestionRecord> questions = new QuizRepository().GetForChapter(ChapterID, IsAdmin);
                pnlQuiz.Visible = true;
                lblChapterID.Text = lesson.ChapterID.ToString();
                lblChapterTitle.Text = Server.HtmlEncode(lesson.ChapterTitle);
                lnkBreadcrumbCourse.Text = Server.HtmlEncode(lesson.CourseTitle);
                lnkBreadcrumbCourse.NavigateUrl = "Course.aspx?courseId=" + lesson.CourseID;
                lnkBreadcrumbChapter.Text = Server.HtmlEncode(lesson.ChapterTitle);
                lnkBreadcrumbChapter.NavigateUrl = "Chapter.aspx?chapterId=" + lesson.ChapterID;
                lnkBackToChapter.NavigateUrl = "Chapter.aspx?chapterId=" + lesson.ChapterID;
                BindQuestions(questions);
                pnlNoQuiz.Visible = questions.Count == 0;
                pnlQuestions.Visible = questions.Count > 0;

                if (isLearner && questions.Count > 0 && !IsRetake)
                {
                    IDictionary<int, string> savedAnswers = RestoreSavedAnswers(userID, lesson.ChapterID);
                    if (savedAnswers.Count > 0)
                    {
                        ShowSavedAttempt(lesson, questions, savedAnswers);
                    }
                }
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowError("The quiz could not be loaded. Run Database/Progress_Extension.sql, then try again.");
            }
        }

        private void BindQuestions(IList<QuizQuestionRecord> questions)
        {
            rptQuizzes.DataSource = questions;
            rptQuizzes.DataBind();
        }

        protected void btnSubmitQuiz_Click(object sender, EventArgs e)
        {
            bool isLearner = string.Equals(Convert.ToString(Session["UserRole"]), "Learner", StringComparison.OrdinalIgnoreCase);
            if (!isLearner && !IsAdmin)
            {
                ShowError("Only learner or administrator accounts can submit chapter quizzes.");
                return;
            }

            int userID = 0;
            if (isLearner && (!int.TryParse(Convert.ToString(Session["UserID"]), out userID) || userID <= 0))
            {
                ShowError("Your learner session has expired. Sign in again before submitting the quiz.");
                return;
            }

            try
            {
                ChapterLessonRecord lesson = new ChapterContentRepository().GetChapter(ChapterID, IsAdmin);
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

                IList<QuizQuestionRecord> questions = new QuizRepository().GetForChapter(ChapterID, IsAdmin);
                if (questions.Count == 0)
                {
                    ShowError("This chapter does not have any quiz questions yet.");
                    return;
                }

                int correct = 0;
                ProgressRepository progress = isLearner ? new ProgressRepository() : null;
                for (int index = 0; index < questions.Count && index < rptQuizzes.Items.Count; index++)
                {
                    QuizQuestionRecord question = questions[index];
                    RadioButtonList answers = (RadioButtonList)rptQuizzes.Items[index].FindControl("rblAnswers");
                    HiddenField quizIDField = (HiddenField)rptQuizzes.Items[index].FindControl("hidQuizID");
                    string selected = answers == null ? null : answers.SelectedValue;
                    int quizID = question.QuizID;
                    int parsedQuizID;
                    if (quizIDField != null && int.TryParse(quizIDField.Value, out parsedQuizID))
                    {
                        quizID = parsedQuizID;
                    }

                    bool isCorrect = !string.IsNullOrWhiteSpace(selected)
                        && string.Equals(selected.Trim(), question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
                    if (isCorrect)
                    {
                        correct++;
                    }

                    if (isLearner)
                    {
                        progress.RecordQuizAttempt(userID, ChapterID, quizID, selected, isCorrect);
                    }
                }

                decimal scorePercent = CalculateScorePercent(correct, questions.Count);
                bool passed = scorePercent >= 75m;
                bool courseCompleted = false;
                if (passed && isLearner)
                {
                    courseCompleted = new EnrollmentRepository().CompleteCourseIfReady(userID, lesson.CourseID);
                }

                pnlResult.Visible = true;
                lblResult.Text = passed
                    ? "Score: " + correct + "/" + questions.Count + " (" + scorePercent.ToString("0.#") + "%). Quiz passed. You can retake it or continue to the next chapter."
                    : "Score: " + correct + "/" + questions.Count + " (" + scorePercent.ToString("0.#") + "%). You need at least 75% to pass. Please retake the quiz.";
                lblSaveNotice.Visible = false;
                btnSubmitQuiz.Visible = false;
                ConfigureRetakeLink();
                lnkNextChapter.Visible = false;

                if (passed)
                {
                    ConfigureNextChapterLink(lesson, courseCompleted);
                }
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                pnlResult.Visible = true;
                lblResult.Text = "Your score could not be saved yet.";
                lblSaveNotice.Text = " Run Database/Progress_Extension.sql against CodeQuestDB, then submit again.";
                lblSaveNotice.Visible = true;
            }
        }

        private void ShowSavedAttempt(
            ChapterLessonRecord lesson,
            IList<QuizQuestionRecord> questions,
            IDictionary<int, string> savedAnswers)
        {
            int correct = CountCorrectAnswers(questions, savedAnswers);
            decimal scorePercent = CalculateScorePercent(correct, questions.Count);
            bool passed = scorePercent >= 75m;
            bool courseCompleted = passed
                && new EnrollmentRepository().CompleteCourseIfReady(Convert.ToInt32(Session["UserID"]), lesson.CourseID);

            pnlResult.Visible = true;
            lblResult.Text = passed
                ? "Latest score: " + correct + "/" + questions.Count + " (" + scorePercent.ToString("0.#") + "%). Quiz passed. You can retake it or continue."
                : "Latest score: " + correct + "/" + questions.Count + " (" + scorePercent.ToString("0.#") + "%). You need at least 75% to pass. Please retake the quiz.";
            lblSaveNotice.Visible = false;
            btnSubmitQuiz.Visible = false;
            ConfigureRetakeLink();
            lnkNextChapter.Visible = false;
            if (passed)
            {
                ConfigureNextChapterLink(lesson, courseCompleted);
            }
        }

        private static int CountCorrectAnswers(
            IList<QuizQuestionRecord> questions,
            IDictionary<int, string> savedAnswers)
        {
            int correct = 0;
            foreach (QuizQuestionRecord question in questions)
            {
                string selected;
                if (savedAnswers.TryGetValue(question.QuizID, out selected)
                    && !string.IsNullOrWhiteSpace(selected)
                    && string.Equals(selected.Trim(), question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    correct++;
                }
            }

            return correct;
        }

        private static decimal CalculateScorePercent(int correct, int total)
        {
            return total <= 0 ? 0m : ((decimal)correct / total) * 100m;
        }

        private void ConfigureRetakeLink()
        {
            lnkRetakeQuiz.Visible = true;
            lnkRetakeQuiz.NavigateUrl = "Quiz.aspx?chapterId=" + ChapterID + "&retake=1";
        }

        private void ConfigureNextChapterLink(ChapterLessonRecord lesson, bool courseCompleted)
        {
            int? nextChapterID = new ChapterContentRepository().GetNextChapterID(lesson.ChapterID, IsAdmin);
            lnkNextChapter.Visible = true;
            if (nextChapterID.HasValue)
            {
                lnkNextChapter.Text = "Next chapter &rarr;";
                lnkNextChapter.NavigateUrl = "Chapter.aspx?chapterId=" + nextChapterID.Value;
                return;
            }

            lnkNextChapter.Text = courseCompleted ? "Back to completed course &rarr;" : "Back to course &rarr;";
            lnkNextChapter.NavigateUrl = "Course.aspx?courseId=" + lesson.CourseID;
        }

        private IDictionary<int, string> RestoreSavedAnswers(int userID, int chapterID)
        {
            IDictionary<int, string> savedAnswers = new ProgressRepository().GetLatestQuizAnswers(userID, chapterID);
            foreach (RepeaterItem item in rptQuizzes.Items)
            {
                HiddenField quizIDField = item.FindControl("hidQuizID") as HiddenField;
                RadioButtonList answers = item.FindControl("rblAnswers") as RadioButtonList;
                int quizID;
                if (quizIDField == null || answers == null || !int.TryParse(quizIDField.Value, out quizID))
                {
                    continue;
                }

                string savedAnswer;
                if (savedAnswers.TryGetValue(quizID, out savedAnswer)
                    && !string.IsNullOrWhiteSpace(savedAnswer)
                    && answers.Items.FindByValue(savedAnswer) != null)
                {
                    answers.SelectedValue = savedAnswer;
                }
            }

            return savedAnswers;
        }

        private void ShowError(string message)
        {
            lblError.Text = Server.HtmlEncode(message);
            pnlError.Visible = true;
        }
    }
}
