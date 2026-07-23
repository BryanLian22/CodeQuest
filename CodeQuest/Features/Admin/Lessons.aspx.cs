// Purpose: Coordinates administrator tutorial, exercise and chapter-quiz authoring and editing.
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
                    LoadQuizCourses(null, null, null);
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
                ResetTutorialForm();
                ResetExerciseForm();
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
                ResetQuizForm();
                int moduleID;
                int chapterID;
                LoadChapters(
                    TryGetSelectedID(ddlQuizModules, out moduleID) ? (int?)moduleID : null,
                    TryGetSelectedID(ddlChapters, out chapterID) ? (int?)chapterID : null);
                HideMessages();
            }
            catch (Exception exception)
            {
                HandleDataException(exception);
            }
        }

        protected void ddlQuizCourses_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ResetQuizForm();
                int courseID;
                if (TryGetSelectedID(ddlQuizCourses, out courseID))
                {
                    lblSelectedQuizCourse.Text = "COURSE-" + courseID;
                    LoadQuizModules(courseID, null, null);
                }
                else
                {
                    lblSelectedQuizCourse.Text = string.Empty;
                    LoadQuizModules(null, null, null);
                }
                HideMessages();
            }
            catch (Exception exception)
            {
                HandleDataException(exception);
            }
        }

        protected void ddlQuizModules_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ResetQuizForm();
                int moduleID;
                if (TryGetSelectedID(ddlQuizModules, out moduleID))
                {
                    lblSelectedQuizModule.Text = "MODULE-" + moduleID;
                    LoadChapters(moduleID, null);
                }
                else
                {
                    lblSelectedQuizModule.Text = string.Empty;
                    LoadChapters(null, null);
                }
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
                int editTutorialID;
                if (TryGetHiddenID(hdnEditTutorialID, out editTutorialID))
                {
                    if (!new AdminContentRepository().UpdateTutorial(
                        editTutorialID,
                        txtTutorialTitle.Text,
                        ddlTutorialCategory.SelectedValue,
                        txtTutorialMaterials.Text,
                        ddlTutorialStatus.SelectedValue))
                    {
                        ShowError("The selected tutorial could not be found.");
                        return;
                    }

                    ResetTutorialForm();
                    ShowSuccess("Tutorial details updated.");
                    LoadTutorials(editTutorialID);
                    return;
                }

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
                int editExerciseID;
                if (TryGetHiddenID(hdnEditExerciseID, out editExerciseID))
                {
                    if (!new AdminContentRepository().UpdateExercise(
                        editExerciseID, tutorialID, txtExerciseQuestion.Text, txtExerciseAnswer.Text))
                    {
                        ShowError("The selected exercise could not be found in this tutorial.");
                        return;
                    }

                    ResetExerciseForm();
                    ShowSuccess("Exercise details updated.");
                    LoadTutorials(tutorialID);
                    return;
                }

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
            int moduleID;
            int chapterID;
            if (!TryGetSelectedID(ddlQuizModules, out moduleID) ||
                !TryGetSelectedID(ddlChapters, out chapterID))
            {
                ShowError("Select a course, module and chapter before creating a quiz.");
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
                int editQuizID;
                if (TryGetHiddenID(hdnEditQuizID, out editQuizID))
                {
                    if (!new AdminContentRepository().UpdateQuiz(
                        editQuizID,
                        chapterID,
                        txtQuizDescription.Text,
                        txtQuizQuestion.Text,
                        txtQuizCorrectAnswer.Text,
                        answers))
                    {
                        ShowError("The selected quiz could not be found in this chapter.");
                        return;
                    }

                    ResetQuizForm();
                    ShowSuccess("Quiz and answer choices updated.");
                    LoadChapters(moduleID, chapterID);
                    return;
                }

                new AdminContentRepository().CreateQuiz(
                    chapterID, txtQuizDescription.Text, txtQuizQuestion.Text, txtQuizCorrectAnswer.Text, answers);
                txtQuizDescription.Text = string.Empty;
                txtQuizQuestion.Text = string.Empty;
                txtQuizCorrectAnswer.Text = string.Empty;
                txtQuizAnswers.Text = string.Empty;
                ShowSuccess("Chapter quiz created with " + answers.Count + " answer choices.");
                LoadChapters(moduleID, chapterID);
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
                ResetTutorialForm();
                ShowSuccess(message);
                LoadTutorials(tutorialID);
            }
            catch (Exception exception)
            {
                HandleDataException(exception);
            }
        }

        protected void btnEditTutorial_Click(object sender, EventArgs e)
        {
            int tutorialID;
            if (!TryGetSelectedID(ddlTutorials, out tutorialID))
            {
                ShowError("Select a tutorial before editing it.");
                return;
            }

            try
            {
                AdminTutorialRecord tutorial = FindTutorial(
                    new AdminContentRepository().GetTutorialsForAdmin(), tutorialID);
                if (tutorial == null)
                {
                    ShowError("The selected tutorial could not be found.");
                    return;
                }

                hdnEditTutorialID.Value = tutorial.TutorialID.ToString();
                txtTutorialTitle.Text = tutorial.Title;
                txtTutorialMaterials.Text = tutorial.Materials ?? string.Empty;
                SelectValue(ddlTutorialCategory, tutorial.Category);
                SelectValue(ddlTutorialStatus, tutorial.Status);
                lblTutorialFormMode.Text = "Editing TUTORIAL-" + tutorial.TutorialID;
                btnCreateTutorial.Text = "Save tutorial changes";
                btnResetTutorial.Visible = true;
                ShowSuccess("Tutorial loaded into the editor.");
            }
            catch (Exception exception)
            {
                HandleDataException(exception);
            }
        }

        protected void btnResetTutorial_Click(object sender, EventArgs e)
        {
            ResetTutorialForm();
            HideMessages();
        }

        protected void btnEditExercise_Command(object sender, CommandEventArgs e)
        {
            int tutorialID;
            int exerciseID;
            if (!TryGetSelectedID(ddlTutorials, out tutorialID) ||
                !int.TryParse(Convert.ToString(e.CommandArgument), out exerciseID) ||
                exerciseID <= 0)
            {
                ShowError("Select a valid exercise before editing it.");
                return;
            }

            try
            {
                AdminExerciseRecord exercise = FindExercise(
                    new AdminContentRepository().GetExercisesForTutorial(tutorialID), exerciseID);
                if (exercise == null)
                {
                    ShowError("The selected exercise could not be found in this tutorial.");
                    return;
                }

                hdnEditExerciseID.Value = exercise.ExerciseID.ToString();
                txtExerciseQuestion.Text = exercise.Question;
                txtExerciseAnswer.Text = exercise.CorrectAnswer;
                lblExerciseFormMode.Text = "Editing EXERCISE-" + exercise.ExerciseID;
                btnCreateExercise.Text = "Save exercise changes";
                btnResetExercise.Visible = true;
                ShowSuccess("Exercise loaded into the editor.");
            }
            catch (Exception exception)
            {
                HandleDataException(exception);
            }
        }

        protected void btnResetExercise_Click(object sender, EventArgs e)
        {
            ResetExerciseForm();
            HideMessages();
        }

        protected void btnEditQuiz_Command(object sender, CommandEventArgs e)
        {
            int chapterID;
            int quizID;
            if (!TryGetSelectedID(ddlChapters, out chapterID) ||
                !int.TryParse(Convert.ToString(e.CommandArgument), out quizID) ||
                quizID <= 0)
            {
                ShowError("Select a valid quiz before editing it.");
                return;
            }

            try
            {
                AdminContentRepository repository = new AdminContentRepository();
                AdminChapterQuizRecord quiz = FindQuiz(repository.GetQuizzesForChapter(chapterID), quizID);
                if (quiz == null)
                {
                    ShowError("The selected quiz could not be found in this chapter.");
                    return;
                }

                hdnEditQuizID.Value = quiz.QuizID.ToString();
                txtQuizDescription.Text = quiz.Description ?? string.Empty;
                txtQuizQuestion.Text = quiz.Question;
                txtQuizCorrectAnswer.Text = quiz.CorrectAnswer;
                txtQuizAnswers.Text = JoinAnswers(repository.GetQuizAnswers(quiz.QuizID));
                lblQuizFormMode.Text = "Editing QUIZ-" + quiz.QuizID;
                btnCreateQuiz.Text = "Save quiz and answers";
                btnResetQuiz.Visible = true;
                ShowSuccess("Quiz and its answer choices loaded into the editor.");
            }
            catch (Exception exception)
            {
                HandleDataException(exception);
            }
        }

        protected void btnResetQuiz_Click(object sender, EventArgs e)
        {
            ResetQuizForm();
            HideMessages();
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
                lnkPreviewTutorial.NavigateUrl = "../Public/Tutorial.aspx?tutorialId=" + tutorialID;
                lnkPreviewTutorial.Visible = true;
                btnEditTutorial.Visible = true;
                rptExercises.DataSource = new AdminContentRepository().GetExercisesForTutorial(tutorialID);
                rptExercises.DataBind();
            }
            else
            {
                pnlNoTutorial.Visible = true;
                pnlExerciseEditor.Visible = false;
                lblSelectedTutorial.Text = string.Empty;
                lnkPreviewTutorial.Visible = false;
                btnEditTutorial.Visible = false;
            }
        }

        private void LoadQuizCourses(int? selectedCourseID, int? selectedModuleID, int? selectedChapterID)
        {
            IList<AdminCourseRecord> courses = new AdminContentRepository().GetCourses();
            ddlQuizCourses.DataSource = courses;
            ddlQuizCourses.DataTextField = "Title";
            ddlQuizCourses.DataValueField = "CourseID";
            ddlQuizCourses.DataBind();
            ddlQuizCourses.Items.Insert(0, new ListItem("Select a course...", ""));

            if (selectedCourseID.HasValue &&
                ddlQuizCourses.Items.FindByValue(selectedCourseID.Value.ToString()) != null)
            {
                ddlQuizCourses.SelectedValue = selectedCourseID.Value.ToString();
            }
            else if (ddlQuizCourses.Items.Count > 1)
            {
                ddlQuizCourses.SelectedIndex = 1;
            }

            int courseID;
            if (TryGetSelectedID(ddlQuizCourses, out courseID))
            {
                lblSelectedQuizCourse.Text = "COURSE-" + courseID;
                LoadQuizModules(courseID, selectedModuleID, selectedChapterID);
            }
            else
            {
                lblSelectedQuizCourse.Text = string.Empty;
                LoadQuizModules(null, null, null);
            }
        }

        private void LoadQuizModules(int? courseID, int? selectedModuleID, int? selectedChapterID)
        {
            IList<AdminModuleRecord> modules = courseID.HasValue
                ? new AdminContentRepository().GetModules(courseID.Value)
                : new List<AdminModuleRecord>();

            ddlQuizModules.DataSource = modules;
            ddlQuizModules.DataTextField = "Title";
            ddlQuizModules.DataValueField = "ModuleID";
            ddlQuizModules.DataBind();
            ddlQuizModules.Items.Insert(0, new ListItem(
                courseID.HasValue ? "Select a module..." : "Select a course first...", ""));

            if (selectedModuleID.HasValue &&
                ddlQuizModules.Items.FindByValue(selectedModuleID.Value.ToString()) != null)
            {
                ddlQuizModules.SelectedValue = selectedModuleID.Value.ToString();
            }
            else if (ddlQuizModules.Items.Count > 1)
            {
                ddlQuizModules.SelectedIndex = 1;
            }

            int moduleID;
            if (TryGetSelectedID(ddlQuizModules, out moduleID))
            {
                lblSelectedQuizModule.Text = "MODULE-" + moduleID;
                LoadChapters(moduleID, selectedChapterID);
            }
            else
            {
                lblSelectedQuizModule.Text = string.Empty;
                LoadChapters(null, null);
            }
        }

        private void LoadChapters(int? moduleID, int? selectedChapterID)
        {
            IList<AdminChapterRecord> chapters = moduleID.HasValue
                ? new AdminContentRepository().GetChapters(moduleID.Value)
                : new List<AdminChapterRecord>();

            ddlChapters.DataSource = chapters;
            ddlChapters.DataTextField = "Title";
            ddlChapters.DataValueField = "ChapterID";
            ddlChapters.DataBind();
            ddlChapters.Items.Insert(0, new ListItem(
                moduleID.HasValue ? "Select a chapter..." : "Select a module first...", ""));

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
                lnkPreviewChapter.NavigateUrl = "../Learner/Chapter.aspx?chapterId=" + chapterID;
                lnkPreviewChapter.Visible = true;
                lnkPreviewQuiz.NavigateUrl = "../Learner/Quiz.aspx?chapterId=" + chapterID;
                lnkPreviewQuiz.Visible = true;
                rptQuizzes.DataSource = new AdminContentRepository().GetQuizzesForChapter(chapterID);
                rptQuizzes.DataBind();
            }
            else
            {
                pnlNoChapter.Visible = true;
                lblSelectedChapter.Text = string.Empty;
                lnkPreviewChapter.Visible = false;
                lnkPreviewQuiz.Visible = false;
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

        private static bool TryGetHiddenID(HiddenField field, out int value)
        {
            return int.TryParse(field.Value, out value) && value > 0;
        }

        private static AdminTutorialRecord FindTutorial(IList<AdminTutorialRecord> tutorials, int tutorialID)
        {
            foreach (AdminTutorialRecord tutorial in tutorials)
            {
                if (tutorial.TutorialID == tutorialID) return tutorial;
            }

            return null;
        }

        private static AdminExerciseRecord FindExercise(IList<AdminExerciseRecord> exercises, int exerciseID)
        {
            foreach (AdminExerciseRecord exercise in exercises)
            {
                if (exercise.ExerciseID == exerciseID) return exercise;
            }

            return null;
        }

        private static AdminChapterQuizRecord FindQuiz(IList<AdminChapterQuizRecord> quizzes, int quizID)
        {
            foreach (AdminChapterQuizRecord quiz in quizzes)
            {
                if (quiz.QuizID == quizID) return quiz;
            }

            return null;
        }

        private static string JoinAnswers(IList<QuizAnswerRecord> answers)
        {
            StringBuilder builder = new StringBuilder();
            foreach (QuizAnswerRecord answer in answers)
            {
                if (builder.Length > 0) builder.AppendLine();
                builder.Append(answer.Answer);
            }

            return builder.ToString();
        }

        private static void SelectValue(DropDownList list, string value)
        {
            if (list.Items.FindByValue(value) != null)
            {
                list.SelectedValue = value;
            }
        }

        private void ResetTutorialForm()
        {
            hdnEditTutorialID.Value = string.Empty;
            txtTutorialTitle.Text = string.Empty;
            txtTutorialMaterials.Text = string.Empty;
            SelectValue(ddlTutorialCategory, "HTML");
            SelectValue(ddlTutorialStatus, "Draft");
            lblTutorialFormMode.Text = "New tutorial";
            btnCreateTutorial.Text = "Create tutorial";
            btnResetTutorial.Visible = false;
        }

        private void ResetExerciseForm()
        {
            hdnEditExerciseID.Value = string.Empty;
            txtExerciseQuestion.Text = string.Empty;
            txtExerciseAnswer.Text = string.Empty;
            lblExerciseFormMode.Text = "New exercise";
            btnCreateExercise.Text = "Add exercise";
            btnResetExercise.Visible = false;
        }

        private void ResetQuizForm()
        {
            hdnEditQuizID.Value = string.Empty;
            txtQuizDescription.Text = string.Empty;
            txtQuizQuestion.Text = string.Empty;
            txtQuizCorrectAnswer.Text = string.Empty;
            txtQuizAnswers.Text = string.Empty;
            lblQuizFormMode.Text = "New quiz";
            btnCreateQuiz.Text = "Create quiz";
            btnResetQuiz.Visible = false;
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
