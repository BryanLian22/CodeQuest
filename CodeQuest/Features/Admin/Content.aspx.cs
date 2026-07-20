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
    public partial class Content : System.Web.UI.Page
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
                    LoadCourses(null);
                }
                catch (ConfigurationErrorsException)
                {
                    ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
                }
                catch (SqlException)
                {
                    ShowError("The content studio could not connect to CodeQuestDB.");
                }
            }
        }

        protected void ddlCourses_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int courseID;
                LoadModules(TryGetSelectedID(ddlCourses, out courseID) ? (int?)courseID : null, null);
                HideMessages();
            }
            catch (Exception exception)
            {
                HandleDataException(exception);
            }
        }

        protected void ddlModules_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int moduleID;
                if (TryGetSelectedID(ddlModules, out moduleID))
                {
                    LoadChapters(moduleID);
                }
                else
                {
                    pnlChapterEditor.Visible = false;
                    pnlNoModule.Visible = true;
                }
                HideMessages();
            }
            catch (Exception exception)
            {
                HandleDataException(exception);
            }
        }

        protected void btnCreateCourse_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCourseTitle.Text))
            {
                ShowError("Enter a course title before creating the course.");
                return;
            }

            try
            {
                int? adminID = GetAdminUserID();
                if (!adminID.HasValue)
                {
                    ShowError("The signed-in Admin is not linked to a dbo.User record. Run Seed_Demo_Content.sql or sign in with a database Admin account.");
                    return;
                }

                int courseID = new AdminContentRepository().CreateCourse(
                    adminID.Value, txtCourseTitle.Text, txtCourseDescription.Text, ddlCourseDifficulty.SelectedValue);
                txtCourseTitle.Text = string.Empty;
                txtCourseDescription.Text = string.Empty;
                ShowSuccess("Course created. Add its first module below.");
                LoadCourses(courseID);
            }
            catch (Exception exception)
            {
                HandleDataException(exception);
            }
        }

        protected void btnCreateModule_Click(object sender, EventArgs e)
        {
            int courseID;
            if (!TryGetSelectedID(ddlCourses, out courseID))
            {
                ShowError("Select a course before adding a module.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtModuleTitle.Text))
            {
                ShowError("Enter a module title before adding the module.");
                return;
            }

            try
            {
                int moduleID = new AdminContentRepository().CreateModule(
                    courseID, txtModuleTitle.Text, txtModuleDescription.Text, ddlModuleStatus.SelectedValue);
                txtModuleTitle.Text = string.Empty;
                txtModuleDescription.Text = string.Empty;
                ShowSuccess("Module added. Select it to add chapters.");
                LoadModules(courseID, moduleID);
            }
            catch (Exception exception)
            {
                HandleDataException(exception);
            }
        }

        protected void btnCreateChapter_Click(object sender, EventArgs e)
        {
            int moduleID;
            if (!TryGetSelectedID(ddlModules, out moduleID))
            {
                ShowError("Select a module before adding a chapter.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtChapterTitle.Text))
            {
                ShowError("Enter a chapter title before adding the chapter.");
                return;
            }

            try
            {
                new AdminContentRepository().CreateChapter(moduleID, txtChapterTitle.Text, txtChapterDescription.Text);
                txtChapterTitle.Text = string.Empty;
                txtChapterDescription.Text = string.Empty;
                ShowSuccess("Chapter added to the selected module.");
                LoadChapters(moduleID);
            }
            catch (Exception exception)
            {
                HandleDataException(exception);
            }
        }

        protected void btnModuleStatus_Command(object sender, CommandEventArgs e)
        {
            int moduleID;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out moduleID))
            {
                ShowError("The module link is missing a valid ID.");
                return;
            }

            try
            {
                string status = string.Equals(Convert.ToString(e.CommandName), "Archive", StringComparison.OrdinalIgnoreCase)
                    ? "Archived"
                    : "Published";
                new AdminContentRepository().UpdateModuleStatus(moduleID, status);
                int courseID;
                if (TryGetSelectedID(ddlCourses, out courseID))
                {
                    LoadModules(courseID, moduleID);
                }
                ShowSuccess(status == "Published" ? "Module published to enrolled learners." : "Module archived.");
            }
            catch (Exception exception)
            {
                HandleDataException(exception);
            }
        }

        private void LoadCourses(int? selectedCourseID)
        {
            IList<AdminCourseRecord> courses = new AdminContentRepository().GetCourses();
            ddlCourses.DataSource = courses;
            ddlCourses.DataTextField = "Title";
            ddlCourses.DataValueField = "CourseID";
            ddlCourses.DataBind();
            ddlCourses.Items.Insert(0, new ListItem("Select a course...", ""));

            if (selectedCourseID.HasValue && ddlCourses.Items.FindByValue(selectedCourseID.Value.ToString()) != null)
            {
                ddlCourses.SelectedValue = selectedCourseID.Value.ToString();
            }
            else if (ddlCourses.Items.Count > 1)
            {
                ddlCourses.SelectedIndex = 1;
            }

            int courseID;
            if (TryGetSelectedID(ddlCourses, out courseID))
            {
                LoadModules(courseID, null);
            }
            else
            {
                pnlModuleEditor.Visible = false;
                pnlNoCourse.Visible = true;
                pnlChapterEditor.Visible = false;
                pnlNoModule.Visible = true;
                lblSelectedCourse.Text = string.Empty;
            }
        }

        private void LoadModules(int? courseID, int? selectedModuleID)
        {
            if (!courseID.HasValue)
            {
                pnlModuleEditor.Visible = false;
                pnlNoCourse.Visible = true;
                pnlChapterEditor.Visible = false;
                pnlNoModule.Visible = true;
                return;
            }

            pnlNoCourse.Visible = false;
            pnlModuleEditor.Visible = true;
            lblSelectedCourse.Text = "Course ID " + courseID.Value;

            IList<AdminModuleRecord> modules = new AdminContentRepository().GetModules(courseID.Value);
            ddlModules.DataSource = modules;
            ddlModules.DataTextField = "Title";
            ddlModules.DataValueField = "ModuleID";
            ddlModules.DataBind();
            ddlModules.Items.Insert(0, new ListItem("Select a module...", ""));

            if (selectedModuleID.HasValue && ddlModules.Items.FindByValue(selectedModuleID.Value.ToString()) != null)
            {
                ddlModules.SelectedValue = selectedModuleID.Value.ToString();
            }
            else if (ddlModules.Items.Count > 1)
            {
                ddlModules.SelectedIndex = 1;
            }

            rptModules.DataSource = modules;
            rptModules.DataBind();

            int moduleID;
            if (TryGetSelectedID(ddlModules, out moduleID))
            {
                LoadChapters(moduleID);
            }
            else
            {
                pnlChapterEditor.Visible = false;
                pnlNoModule.Visible = true;
                lblSelectedModule.Text = string.Empty;
            }
        }

        private void LoadChapters(int moduleID)
        {
            pnlNoModule.Visible = false;
            pnlChapterEditor.Visible = true;
            lblSelectedModule.Text = "Module ID " + moduleID;
            rptChapters.DataSource = new AdminContentRepository().GetChapters(moduleID);
            rptChapters.DataBind();
        }

        private int? GetAdminUserID()
        {
            int userID;
            if (int.TryParse(Convert.ToString(Session["UserID"]), out userID) && userID > 0)
            {
                return userID;
            }

            string email = Convert.ToString(Session["UserEmail"]);
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            UserRecord user = new UserRepository().FindByEmail(email);
            return user != null && string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase)
                ? (int?)user.UserID
                : null;
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
            }
            else
            {
                SqlException sqlException = exception as SqlException;
                if (sqlException != null && (sqlException.Number == 2601 || sqlException.Number == 2627))
                {
                    ShowError("That course title already exists. Choose a different title.");
                }
                else if (sqlException != null)
                {
                    ShowError("The content change could not be saved to CodeQuestDB.");
                }
                else
                {
                    ShowError("The content studio could not complete that action.");
                }
            }
        }
    }
}
