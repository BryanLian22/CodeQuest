using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using CodeQuest.Data;
using CodeQuest.Data.Repositories;
using CodeQuest.Models;

namespace CodeQuest.Features.AI
{
    public partial class Assistant : System.Web.UI.Page
    {
        private const string ConversationSessionKey = "CodeQuestAiConversation";
        private int ChapterID
        {
            get
            {
                int chapterID;
                return int.TryParse(Request.QueryString["chapterId"], out chapterID) && chapterID > 0 ? chapterID : 0;
            }
        }

        private int UserID
        {
            get
            {
                int userID;
                return int.TryParse(Convert.ToString(Session["UserID"]), out userID) && userID > 0 ? userID : 0;
            }
        }

        private IList<AiChatMessage> Conversation
        {
            get
            {
                IList<AiChatMessage> messages = Session[ConversationSessionKey] as IList<AiChatMessage>;
                if (messages == null)
                {
                    messages = new List<AiChatMessage>();
                    Session[ConversationSessionKey] = messages;
                }

                return messages;
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
            if (!string.Equals(Convert.ToString(Session["UserRole"]), "Learner", StringComparison.OrdinalIgnoreCase))
            {
                Session["ReturnUrl"] = Request.RawUrl;
                Response.Redirect("../../Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadAssistant();
            }
        }

        protected void btnAsk_Click(object sender, EventArgs e)
        {
            pnlError.Visible = false;
            try
            {
                if (!EnsurePremium())
                {
                    return;
                }
                EnsureConversationContext();
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
                return;
            }
            catch (SqlException)
            {
                ShowError("The AI assistant could not load your account from CodeQuestDB.");
                return;
            }

            string prompt = (txtPrompt.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(prompt))
            {
                ShowError("Write a question before asking the assistant.");
                return;
            }

            if (prompt.Length > 2000)
            {
                ShowError("Keep the prompt within 2,000 characters.");
                return;
            }

            IList<AiChatMessage> messages = Conversation;
            messages.Add(new AiChatMessage { Role = "user", Content = prompt });
            try
            {
                string answer = new GoogleAiClient().Ask(GetContextSummary(), messages);
                messages.Add(new AiChatMessage { Role = "assistant", Content = answer });
                txtPrompt.Text = string.Empty;
                BindConversation();
            }
            catch (ConfigurationErrorsException)
            {
                messages.RemoveAt(messages.Count - 1);
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                messages.RemoveAt(messages.Count - 1);
                ShowError("The learning context could not be loaded from CodeQuestDB.");
            }
            catch (System.Net.WebException)
            {
                messages.RemoveAt(messages.Count - 1);
                ShowError("The AI service could not be reached. Try again in a moment.");
            }
            catch (InvalidOperationException exception)
            {
                messages.RemoveAt(messages.Count - 1);
                ShowError(exception.Message);
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            Session.Remove(ConversationSessionKey);
            try
            {
                if (!EnsurePremium())
                {
                    return;
                }

                EnsureConversationContext();
                pnlAssistant.Visible = true;
                BindContext();
                LoadConversation();
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowError("The AI assistant could not load your account from CodeQuestDB.");
            }
        }

        private void LoadAssistant()
        {
            pnlAssistant.Visible = false;
            pnlLocked.Visible = false;
            pnlError.Visible = false;

            try
            {
                if (!EnsurePremium())
                {
                    return;
                }

                EnsureConversationContext();
                pnlAssistant.Visible = true;
                BindContext();
                LoadConversation();
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowError("The AI assistant could not load your account from CodeQuestDB.");
            }
        }

        private bool EnsurePremium()
        {
            if (UserID <= 0)
            {
                ShowError("This sign-in is not linked to a database learner account.");
                return false;
            }

            UserRecord user = new UserRepository().FindByID(UserID);
            if (user == null || !string.Equals(user.Role, "Learner", StringComparison.OrdinalIgnoreCase))
            {
                ShowError("The learner account could not be found.");
                return false;
            }

            Session["DisplayName"] = user.Username;
            Session["UserEmail"] = user.Email;
            Session["UserPlan"] = user.Plan;
            bool premium = string.Equals(user.Plan, "Premium", StringComparison.OrdinalIgnoreCase);
            pnlLocked.Visible = !premium;
            pnlAssistant.Visible = premium;
            if (!premium)
            {
                return false;
            }

            return true;
        }

        private void BindContext()
        {
            ChapterLessonRecord lesson = null;
            if (ChapterID > 0)
            {
                lesson = new ChapterContentRepository().GetChapter(ChapterID);
                if (lesson != null && !new EnrollmentRepository().IsEnrolled(UserID, lesson.CourseID))
                {
                    lesson = null;
                }
            }

            if (lesson == null)
            {
                lblContextTitle.Text = "General web development";
                lblCourse.Text = "No course selected";
                lblModule.Text = "Ask anything";
                lblChapter.Text = "HTML · CSS · JavaScript";
                return;
            }

            lblContextTitle.Text = Server.HtmlEncode(lesson.ChapterTitle);
            lblCourse.Text = Server.HtmlEncode(lesson.CourseTitle);
            lblModule.Text = Server.HtmlEncode(lesson.ModuleTitle);
            lblChapter.Text = Server.HtmlEncode(lesson.ChapterTitle);
        }

        private void EnsureConversationContext()
        {
            string contextKey = UserID.ToString() + ":" + ChapterID.ToString();
            if (!string.Equals(Convert.ToString(Session["CodeQuestAiConversationContext"]), contextKey, StringComparison.Ordinal))
            {
                Session.Remove(ConversationSessionKey);
                Session["CodeQuestAiConversationContext"] = contextKey;
            }
        }

        private string GetContextSummary()
        {
            return (lblCourse.Text + " / " + lblModule.Text + " / " + lblChapter.Text).Replace("&middot;", "·");
        }

        private void LoadConversation()
        {
            IList<AiChatMessage> messages = Conversation;
            if (messages.Count == 0)
            {
                messages.Add(new AiChatMessage
                {
                    Role = "assistant",
                    Content = "Hi! I can explain the current lesson, show a small example or give you a debugging hint. What are you working on?"
                });
            }

            BindConversation();
        }

        private void BindConversation()
        {
            rptMessages.DataSource = Conversation;
            rptMessages.DataBind();
        }

        private void ShowError(string message)
        {
            lblError.Text = Server.HtmlEncode(message);
            pnlError.Visible = true;
        }
    }
}
