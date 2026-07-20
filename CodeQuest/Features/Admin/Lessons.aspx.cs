using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI.WebControls;
using CodeQuest.Data.Repositories;
using CodeQuest.Models;

namespace CodeQuest.Features.Admin
{
    public partial class Lessons : System.Web.UI.Page
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
                Response.Redirect("../../Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                try
                {
                    LoadTutorials(null);
                    LoadChapters(null);
                }
                catch (ConfigurationErrorsException)
                {
                    ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
                }
                catch (SqlException)
                {
                    ShowError("The lesson library could not connect to CodeQuestDB. Confirm that Progress_Extension.sql has been run.");
                }
            }
        }

        protected void ddlTutorials_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int tutorialID;
                LoadTutorials(TryGetSelectedID(ddlTutorials, out tutorialID) ? (int?)tutorialID : null);
                HideMessages();
            }
            catch (Exception exception)
            {
                HandleDataException(exception);
            }
        }

        protected void ddlChapters_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int chapterID;
                LoadChapters(TryGetSelectedID(ddlChapters, out chapterID) ? (int?)chapterID : null);
                HideMessages();
            }
            catch (Exception exception)
            {
                HandleDataException(exception);
            }
        }

        protected void btnCreateTutorial_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTutorialTitle.Text))
            {
                ShowError("Enter a tutorial title before creating the tutorial.");
                return;
            }

            try
            {
                int tutorialID = new AdminContentRepository().CreateTutorial(
                    txtTutorialTitle.Text, ddlTutorialCategory.SelectedValue, txtTutorialMaterials.Text, ddlTutorialStatus.SelectedValue);
                txtTutorialTitle.Text = string.Empty;
                txtTutorialMaterials.Text = string.Empty;
                ShowSuccess("Tutorial created. Add an exercise below, or publish it when ready.");
                LoadTutorials(tutorialID);
            }
            catch (Exception exception)
            {
                HandleDataException(exception);
            }
        }

        protected void btnCreateExercise_Click(object sender, EventArgs e)
        {
            int tutorialID;
            if (!TryGetSelectedID(ddlTutorials, out tutorialID))
            {
                ShowError("Select a tutorial before adding an exercise.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtExerciseQuestion.Text) || string.IsNullOrWhiteSpace(txtExerciseAnswer.Text))
            {
                ShowError("Enter both an exercise question and its correct answer.");
                return;
            }

            try
            {
                new AdminContentRepository().CreateExercise(tutorialID, txtExerciseQuestion.Text, txtExerciseAnswer.Text);
                txtExerciseQuestion.Text = string.Empty;
                txtExerciseAnswer.Text = string.Empty;
                ShowSuccess("Guest exercise added to the selected tutorial.");
                LoadTutorials(tutorialID);
            }
            catch (Exception exception)
            {
                HandleDataException(exception);
            }
        }

        protected void btnPublishTutorial_Click(object sender, EventArgs e)
        {
            SetTutorialStatus("Published", "Tutorial published. Guests can now read it and try its exercises.");
        }

        protected void btnReviewTutorial_Click(object sender, EventArgs e)
        {
            SetTutorialStatus("Review", "Tutorial moved to review.");
        }

        protected void btnCreateQuiz_Click(object sender, EventArgs e)
        {
            int chapterID;
            if (!TryGetSelectedID(ddlChapters, out chapterID))
            {
                ShowError("Select a chapter before creating a quiz.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtQuizQuestion.Text) || string.IsNullOrWhiteSpace(txtQuizCorrectAnswer.Text))
            {
                ShowError("Enter both a quiz question and its correct answer.");
                return;
            }

            IList<string> answers = ParseAnswers(txtQuizAnswers.Text, txtQuizCorrectAnswer.Text.Trim());
            if (answers.Count < 2)
            {
                ShowError("Add at least two answer choices. The correct answer will be included automatically if needed.");
                return;
            }

            try
            {
                new AdminContentRepository().CreateQuiz(
                    chapterID, txtQuizDescription.Text, txtQuizQuestion.Text, txtQuizCorrectAnswer.Text, answers);
                txtQuizDescription.Text = string.Empty;
                txtQuizQuestion.Text = string.Empty;
                txtQuizCorrectAnswer.Text = string.Empty;
                txtQuizAnswers.Text = string.Empty;
                ShowSuccess("Chapter quiz created with " + answers.Count + " answer choices.");
                LoadChapters(chapterID);
            }
            catch (Exception exception)
            {
                HandleDataException(exception);
            }
        }

        private void SetTutorialStatus(string status, string message)
        {
            int tutorialID;
            if (!TryGetSelectedID(ddlTutorials, out tutorialID))
            {
                ShowError("Select a tutorial before changing its status.");
                return;
            }

            try
            {
                new AdminContentRepository().UpdateTutorialStatus(tutorialID, status);
                ShowSuccess(message);
                LoadTutorials(tutorialID);
            }
            catch (Exception exception)
            {
                HandleDataException(exception);
            }
        }

        private void LoadTutorials(int? selectedTutorialID)
        {
            IList<AdminTutorialRecord> tutorials = new AdminContentRepository().GetTutorialsForAdmin();
            ddlTutorials.DataSource = tutorials;
            ddlTutorials.DataTextField = "Title";
            ddlTutorials.DataValueField = "TutorialID";
            ddlTutorials.DataBind();
            ddlTutorials.Items.Insert(0, new ListItem("Select a tutorial...", ""));

            if (selectedTutorialID.HasValue && ddlTutorials.Items.FindByValue(selectedTutorialID.Value.ToString()) != null)
            {
                ddlTutorials.SelectedValue = selectedTutorialID.Value.ToString();
            }
            else if (ddlTutorials.Items.Count > 1)
            {
                ddlTutorials.SelectedIndex = 1;
            }

            int tutorialID;
            if (TryGetSelectedID(ddlTutorials, out tutorialID))
            {
                pnlNoTutorial.Visible = false;
                pnlExerciseEditor.Visible = true;
                lblSelectedTutorial.Text = "Tutorial ID " + tutorialID;
                rptExercises.DataSource = new AdminContentRepository().GetExercisesForTutorial(tutorialID);
                rptExercises.DataBind();
            }
            else
            {
                pnlNoTutorial.Visible = true;
                pnlExerciseEditor.Visible = false;
                lblSelectedTutorial.Text = string.Empty;
            }
        }

        private void LoadChapters(int? selectedChapterID)
        {
            IList<AdminChapterOptionRecord> chapters = new AdminContentRepository().GetChapterOptions();
            ddlChapters.DataSource = chapters;
            ddlChapters.DataTextField = "ChapterTitle";
            ddlChapters.DataValueField = "ChapterID";
            ddlChapters.DataBind();
            ddlChapters.Items.Insert(0, new ListItem("Select a chapter...", ""));

            if (selectedChapterID.HasValue && ddlChapters.Items.FindByValue(selectedChapterID.Value.ToString()) != null)
            {
                ddlChapters.SelectedValue = selectedChapterID.Value.ToString();
            }
            else if (ddlChapters.Items.Count > 1)
            {
                ddlChapters.SelectedIndex = 1;
            }

            int chapterID;
            if (TryGetSelectedID(ddlChapters, out chapterID))
            {
                pnlNoChapter.Visible = false;
                lblSelectedChapter.Text = "Chapter ID " + chapterID;
                rptQuizzes.DataSource = new AdminContentRepository().GetQuizzesForChapter(chapterID);
                rptQuizzes.DataBind();
            }
            else
            {
                pnlNoChapter.Visible = true;
                lblSelectedChapter.Text = string.Empty;
                rptQuizzes.DataSource = null;
                rptQuizzes.DataBind();
            }
        }

        private static IList<string> ParseAnswers(string raw, string correctAnswer)
        {
            List<string> answers = new List<string>();
            string[] parts = (raw ?? string.Empty).Replace("\r", "\n").Split(new[] { '\n', ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                string answer = part.Trim();
                if (answer.Length > 0 && !ContainsAnswer(answers, answer))
                {
                    answers.Add(answer);
                }
            }

            if (!ContainsAnswer(answers, correctAnswer))
            {
                answers.Add(correctAnswer);
            }

            return answers;
        }

        private static bool ContainsAnswer(IList<string> answers, string value)
        {
            foreach (string answer in answers)
            {
                if (string.Equals(answer, value, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private static bool TryGetSelectedID(DropDownList list, out int value)
        {
            return int.TryParse(list.SelectedValue, out value) && value > 0;
        }

        private void HideMessages()
        {
            pnlError.Visible = false;
            pnlSuccess.Visible = false;
        }

        private void ShowSuccess(string message)
        {
            pnlError.Visible = false;
            lblSuccess.Text = Server.HtmlEncode(message);
            pnlSuccess.Visible = true;
        }

        private void ShowError(string message)
        {
            pnlSuccess.Visible = false;
            lblError.Text = Server.HtmlEncode(message);
            pnlError.Visible = true;
        }

        private void HandleDataException(Exception exception)
        {
            if (exception is ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
                return;
            }

            SqlException sqlException = exception as SqlException;
            if (sqlException != null && (sqlException.Number == 2601 || sqlException.Number == 2627))
            {
                ShowError("That record already exists. Choose a different title or answer.");
            }
            else if (sqlException != null)
            {
                ShowError("The lesson change could not be saved to CodeQuestDB. Confirm that Progress_Extension.sql has been run.");
            }
            else
            {
                ShowError("The lesson library could not complete that action.");
            }
        }
    }
}
