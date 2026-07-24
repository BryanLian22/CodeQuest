// Purpose: Coordinates administrator course, module and chapter creation, editing, publishing and previews.
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
                ResetCourseForm();
                ResetModuleForm();
                ResetChapterForm();
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
                ResetModuleForm();
                ResetChapterForm();
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
                int editCourseID;
                if (TryGetHiddenID(hdnEditCourseID, out editCourseID))
                {
                    if (!new AdminContentRepository().UpdateCourse(
                        editCourseID, txtCourseTitle.Text, txtCourseDescription.Text, ddlCourseDifficulty.SelectedValue))
                    {
                        ShowError("The selected course could not be found.");
                        return;
                    }

                    ResetCourseForm();
                    ShowSuccess("Course details updated.");
                    LoadCourses(editCourseID);
                    return;
                }

                int? adminID = GetAdminUserID();
                if (!adminID.HasValue)
                {
                    ShowError("The signed-in Admin is not linked to a dbo.User record. Enable demo seeding or sign in with a database Admin account.");
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
                int editModuleID;
                if (TryGetHiddenID(hdnEditModuleID, out editModuleID))
                {
                    if (!new AdminContentRepository().UpdateModule(
                        editModuleID,
                        courseID,
                        txtModuleTitle.Text,
                        txtModuleDescription.Text,
                        ddlModuleStatus.SelectedValue))
                    {
                        ShowError("The selected module could not be found in this course.");
                        return;
                    }

                    ResetModuleForm();
                    ShowSuccess("Module details updated.");
                    LoadModules(courseID, editModuleID);
                    return;
                }

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
                int editChapterID;
                if (TryGetHiddenID(hdnEditChapterID, out editChapterID))
                {
                    if (!new AdminContentRepository().UpdateChapter(
                        editChapterID, moduleID, txtChapterTitle.Text, txtChapterDescription.Text))
                    {
                        ShowError("The selected chapter could not be found in this module.");
                        return;
                    }

                    ResetChapterForm();
                    ShowSuccess("Chapter details updated.");
                    LoadChapters(moduleID);
                    return;
                }

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
                ResetModuleForm();
                ResetChapterForm();
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

        protected void btnEditCourse_Click(object sender, EventArgs e)
        {
            int courseID;
            if (!TryGetSelectedID(ddlCourses, out courseID))
            {
                ShowError("Select a course before editing it.");
                return;
            }

            try
            {
                AdminCourseRecord course = FindCourse(new AdminContentRepository().GetCourses(), courseID);
                if (course == null)
                {
                    ShowError("The selected course could not be found.");
                    return;
                }

                hdnEditCourseID.Value = course.CourseID.ToString();
                txtCourseTitle.Text = course.Title;
                txtCourseDescription.Text = course.Description ?? string.Empty;
                SelectValue(ddlCourseDifficulty, course.Difficulty);
                lblCourseFormMode.Text = "Editing COURSE-" + course.CourseID;
                btnCreateCourse.Text = "Save course changes";
                btnResetCourse.Visible = true;
                ShowSuccess("Course loaded into the editor.");
            }
            catch (Exception exception)
            {
                HandleDataException(exception);
            }
        }

        protected void btnResetCourse_Click(object sender, EventArgs e)
        {
            ResetCourseForm();
            HideMessages();
        }

        protected void btnEditModule_Click(object sender, EventArgs e)
        {
            int courseID;
            int moduleID;
            if (!TryGetSelectedID(ddlCourses, out courseID) || !TryGetSelectedID(ddlModules, out moduleID))
            {
                ShowError("Select a module before editing it.");
                return;
            }

            try
            {
                AdminModuleRecord module = FindModule(new AdminContentRepository().GetModules(courseID), moduleID);
                if (module == null)
                {
                    ShowError("The selected module could not be found.");
                    return;
                }

                hdnEditModuleID.Value = module.ModuleID.ToString();
                txtModuleTitle.Text = module.Title;
                txtModuleDescription.Text = module.Description ?? string.Empty;
                SelectValue(ddlModuleStatus, module.Status);
                lblModuleFormMode.Text = "Editing MODULE-" + module.ModuleID;
                btnCreateModule.Text = "Save module changes";
                btnResetModule.Visible = true;
                ShowSuccess("Module loaded into the editor.");
            }
            catch (Exception exception)
            {
                HandleDataException(exception);
            }
        }

        protected void btnResetModule_Click(object sender, EventArgs e)
        {
            ResetModuleForm();
            HideMessages();
        }

        protected void btnEditChapter_Command(object sender, CommandEventArgs e)
        {
            int moduleID;
            int chapterID;
            if (!TryGetSelectedID(ddlModules, out moduleID) ||
                !int.TryParse(Convert.ToString(e.CommandArgument), out chapterID) ||
                chapterID <= 0)
            {
                ShowError("Select a valid chapter before editing it.");
                return;
            }

            try
            {
                AdminChapterRecord chapter = FindChapter(new AdminContentRepository().GetChapters(moduleID), chapterID);
                if (chapter == null)
                {
                    ShowError("The selected chapter could not be found in this module.");
                    return;
                }

                hdnEditChapterID.Value = chapter.ChapterID.ToString();
                txtChapterTitle.Text = chapter.Title;
                txtChapterDescription.Text = chapter.Description ?? string.Empty;
                lblChapterFormMode.Text = "Editing CHAPTER-" + chapter.ChapterID;
                btnCreateChapter.Text = "Save chapter changes";
                btnResetChapter.Visible = true;
                ShowSuccess("Chapter loaded into the editor.");
            }
            catch (Exception exception)
            {
                HandleDataException(exception);
            }
        }

        protected void btnResetChapter_Click(object sender, EventArgs e)
        {
            ResetChapterForm();
            HideMessages();
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
                lnkPreviewCourse.Visible = false;
                btnEditCourse.Visible = false;
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
                lnkPreviewCourse.Visible = false;
                btnEditCourse.Visible = false;
                btnEditModule.Visible = false;
                return;
            }

            pnlNoCourse.Visible = false;
            pnlModuleEditor.Visible = true;
            lblSelectedCourse.Text = "Course ID " + courseID.Value;
            lnkPreviewCourse.NavigateUrl = "../Learner/Course.aspx?courseId=" + courseID.Value;
            lnkPreviewCourse.Visible = true;
            btnEditCourse.Visible = true;

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
                btnEditModule.Visible = true;
                LoadChapters(moduleID);
            }
            else
            {
                btnEditModule.Visible = false;
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

        private static bool TryGetHiddenID(HiddenField field, out int value)
        {
            return int.TryParse(field.Value, out value) && value > 0;
        }

        private static AdminCourseRecord FindCourse(IList<AdminCourseRecord> courses, int courseID)
        {
            foreach (AdminCourseRecord course in courses)
            {
                if (course.CourseID == courseID) return course;
            }

            return null;
        }

        private static AdminModuleRecord FindModule(IList<AdminModuleRecord> modules, int moduleID)
        {
            foreach (AdminModuleRecord module in modules)
            {
                if (module.ModuleID == moduleID) return module;
            }

            return null;
        }

        private static AdminChapterRecord FindChapter(IList<AdminChapterRecord> chapters, int chapterID)
        {
            foreach (AdminChapterRecord chapter in chapters)
            {
                if (chapter.ChapterID == chapterID) return chapter;
            }

            return null;
        }

        private static void SelectValue(DropDownList list, string value)
        {
            if (list.Items.FindByValue(value) != null)
            {
                list.SelectedValue = value;
            }
        }

        private void ResetCourseForm()
        {
            hdnEditCourseID.Value = string.Empty;
            txtCourseTitle.Text = string.Empty;
            txtCourseDescription.Text = string.Empty;
            SelectValue(ddlCourseDifficulty, "Beginner");
            lblCourseFormMode.Text = "New course";
            btnCreateCourse.Text = "Create course";
            btnResetCourse.Visible = false;
        }

        private void ResetModuleForm()
        {
            hdnEditModuleID.Value = string.Empty;
            txtModuleTitle.Text = string.Empty;
            txtModuleDescription.Text = string.Empty;
            SelectValue(ddlModuleStatus, "Draft");
            lblModuleFormMode.Text = "New module";
            btnCreateModule.Text = "Add module";
            btnResetModule.Visible = false;
        }

        private void ResetChapterForm()
        {
            hdnEditChapterID.Value = string.Empty;
            txtChapterTitle.Text = string.Empty;
            txtChapterDescription.Text = string.Empty;
            lblChapterFormMode.Text = "New chapter";
            btnCreateChapter.Text = "Add chapter";
            btnResetChapter.Visible = false;
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
