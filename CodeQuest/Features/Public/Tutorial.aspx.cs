using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using CodeQuest.Data.Repositories;
using CodeQuest.Models;

namespace CodeQuest.Features.Public
{
    public partial class Tutorial : System.Web.UI.Page
    {
        private int TutorialID
        {
            get
            {
                int tutorialID;
                return int.TryParse(Request.QueryString["tutorialId"], out tutorialID) ? tutorialID : 0;
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
            ConfigureHeader();
            if (!IsPostBack)
            {
                LoadTutorial();
            }
        }

        private void ConfigureHeader()
        {
            bool isAdmin = string.Equals(Convert.ToString(Session["UserRole"]), "Admin", StringComparison.OrdinalIgnoreCase);
            phPublicNavigation.Visible = !isAdmin;
            phAdminNavigation.Visible = isAdmin;
            phPublicActions.Visible = !isAdmin;
            phAdminActions.Visible = isAdmin;
            pnlAdminPreview.Visible = isAdmin;
        }

        private void LoadTutorial()
        {
            if (TutorialID <= 0)
            {
                ShowError("The tutorial link is missing a valid tutorial ID.");
                return;
            }

            try
            {
                bool isAdmin = string.Equals(Convert.ToString(Session["UserRole"]), "Admin", StringComparison.OrdinalIgnoreCase);
                TutorialRecord tutorial = new TutorialRepository().GetByID(TutorialID, isAdmin);
                if (tutorial == null)
                {
                    ShowError("That public tutorial could not be found.");
                    return;
                }

                BindTutorial(tutorial);
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowError("The public tutorial could not connect to CodeQuestDB.");
            }
        }

        private void BindTutorial(TutorialRecord tutorial)
        {
                pnlTutorial.Visible = true;
                lblTutorialID.Text = tutorial.TutorialID.ToString();
                lblCategory.Text = Server.HtmlEncode(string.IsNullOrWhiteSpace(tutorial.Category) ? "HTML" : tutorial.Category);
                lblTitle.Text = Server.HtmlEncode(tutorial.Title);

                bool isHtmlStructure = string.Equals(tutorial.Title, "HTML Document Structure", StringComparison.OrdinalIgnoreCase);
                pnlHtmlGuide.Visible = isHtmlStructure;

                if (!isHtmlStructure && !string.IsNullOrWhiteSpace(tutorial.Materials))
                {
                pnlMaterials.Visible = true;
                litMaterials.Text = Server.HtmlEncode(tutorial.Materials.Replace("\\n", Environment.NewLine));
            }
            else
            {
                    pnlNoMaterials.Visible = !isHtmlStructure;
            }

            if (tutorial.Exercises.Count > 0)
            {
                ExerciseRecord exercise = tutorial.Exercises[0];
                pnlExercise.Visible = true;
                lblExerciseQuestion.Text = Server.HtmlEncode(exercise.Question);
                ViewState["CorrectAnswer"] = exercise.CorrectAnswer;
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
