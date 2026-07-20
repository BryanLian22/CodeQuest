using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using CodeQuest.Data.Repositories;
using CodeQuest.Models;

namespace CodeQuest.Features.Public
{
    public partial class Tutorials : System.Web.UI.Page
    {
        private string SelectedCategory
        {
            get
            {
                string category = Convert.ToString(Request.QueryString["category"]);
                if (string.Equals(category, "HTML", StringComparison.OrdinalIgnoreCase)) return "HTML";
                if (string.Equals(category, "CSS", StringComparison.OrdinalIgnoreCase)) return "CSS";
                if (string.Equals(category, "JavaScript", StringComparison.OrdinalIgnoreCase)) return "JavaScript";
                return null;
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
            if (!IsPostBack)
            {
                LoadTutorials();
            }
        }

        private void LoadTutorials()
        {
            try
            {
                IList<TutorialRecord> tutorials = new TutorialRepository().GetPublished(SelectedCategory);
                rptTutorials.DataSource = tutorials;
                rptTutorials.DataBind();
                pnlEmpty.Visible = tutorials.Count == 0;
                lblCategoryTitle.Text = Server.HtmlEncode(SelectedCategory ?? "All tutorials");
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowError("The public tutorials could not connect to CodeQuestDB.");
            }
        }

        private void ShowError(string message)
        {
            lblError.Text = Server.HtmlEncode(message);
            pnlError.Visible = true;
            pnlEmpty.Visible = false;
        }
    }
}
