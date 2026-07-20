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
                    BindQuestions(new QuizRepository().GetForChapter(ChapterID));
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

            if (!string.Equals(Convert.ToString(Session["UserRole"]), "Learner", StringComparison.OrdinalIgnoreCase))
            {
                ShowError("Only learner accounts can open chapter quizzes.");
                return;
            }

            int userID;
            if (!int.TryParse(Convert.ToString(Session["UserID"]), out userID) || userID <= 0)
            {
                ShowError("This sign-in is not linked to a database learner. Register a real account to save quiz progress.");
                return;
            }

            try
            {
                ChapterLessonRecord lesson = new ChapterContentRepository().GetChapter(ChapterID);
                if (lesson == null)
                {
                    ShowError("That chapter could not be found or is not published.");
                    return;
                }

                if (!new EnrollmentRepository().IsEnrolled(userID, lesson.CourseID))
                {
                    Response.Redirect("Course.aspx?courseId=" + lesson.CourseID, false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                IList<QuizQuestionRecord> questions = new QuizRepository().GetForChapter(ChapterID);
                pnlQuiz.Visible = true;
                lblChapterID.Text = lesson.ChapterID.ToString();
                lblChapterTitle.Text = Server.HtmlEncode(lesson.ChapterTitle);
                lblBreadcrumbCourse.Text = Server.HtmlEncode(lesson.CourseTitle);
                lblBreadcrumbChapter.Text = Server.HtmlEncode(lesson.ChapterTitle);
                lnkBackToChapter.NavigateUrl = "Chapter.aspx?chapterId=" + lesson.ChapterID;
                BindQuestions(questions);
                pnlNoQuiz.Visible = questions.Count == 0;
                pnlQuestions.Visible = questions.Count > 0;
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
            if (!string.Equals(Convert.ToString(Session["UserRole"]), "Learner", StringComparison.OrdinalIgnoreCase))
            {
                ShowError("Only learner accounts can submit chapter quizzes.");
                return;
            }

            int userID;
            if (!int.TryParse(Convert.ToString(Session["UserID"]), out userID) || userID <= 0)
            {
                ShowError("Your learner session has expired. Sign in again before submitting the quiz.");
                return;
            }

            try
            {
                ChapterLessonRecord lesson = new ChapterContentRepository().GetChapter(ChapterID);
                if (lesson == null)
                {
                    ShowError("That chapter could not be found or is not published.");
                    return;
                }

                if (!new EnrollmentRepository().IsEnrolled(userID, lesson.CourseID))
                {
                    Response.Redirect("Course.aspx?courseId=" + lesson.CourseID, false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                IList<QuizQuestionRecord> questions = new QuizRepository().GetForChapter(ChapterID);
                if (questions.Count == 0)
                {
                    ShowError("This chapter does not have any quiz questions yet.");
                    return;
                }

                int correct = 0;
                ProgressRepository progress = new ProgressRepository();
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

                    progress.RecordQuizAttempt(userID, ChapterID, quizID, selected, isCorrect);
                }

                bool passed = correct == questions.Count;
                if (passed)
                {
                    progress.MarkChapterCompleted(userID, ChapterID);
                }

                pnlResult.Visible = true;
                lblResult.Text = passed
                    ? "Score: " + correct + "/" + questions.Count + ". Chapter completed and progress saved."
                    : "Score: " + correct + "/" + questions.Count + ". Review the chapter and try the quiz again to complete it.";
                lblSaveNotice.Visible = false;
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

        private void ShowError(string message)
        {
            lblError.Text = Server.HtmlEncode(message);
            pnlError.Visible = true;
        }
    }
}
