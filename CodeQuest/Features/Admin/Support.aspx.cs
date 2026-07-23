// Purpose: Loads support conversations and handles administrator replies and status changes.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using CodeQuest.Data.Repositories;
using CodeQuest.Models;

namespace CodeQuest.Features.Admin
{
    public partial class Support : System.Web.UI.Page
    {
        private int SelectedTicketID
        {
            get
            {
                int ticketID;
                if (int.TryParse(Request.QueryString["ticketId"], out ticketID) && ticketID > 0)
                {
                    ViewState["SelectedTicketID"] = ticketID;
                    return ticketID;
                }

                return int.TryParse(Convert.ToString(ViewState["SelectedTicketID"]), out ticketID) && ticketID > 0
                    ? ticketID
                    : 0;
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
            if (!string.Equals(Convert.ToString(Session["UserRole"]), "Admin", StringComparison.OrdinalIgnoreCase))
            {
                Session["ReturnUrl"] = Request.RawUrl;
                Response.Redirect("../../Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadSupport();
            }
        }

        protected void btnAdminReply_Click(object sender, EventArgs e)
        {
            if (SelectedTicketID <= 0)
            {
                ShowError("Select a valid ticket before sending a reply.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAdminReply.Text))
            {
                ShowError("Enter a reply before sending it.");
                return;
            }

            try
            {
                int userID;
                if (!TryGetAdminUserID(out userID))
                {
                    ShowError("This administrator session is not linked to the admin row in dbo.User. Run Seed_Demo_Content.sql, then sign out and sign in again.");
                    return;
                }

                if (!new SupportRepository().AddReply(SelectedTicketID, userID, txtAdminReply.Text, true))
                {
                    ShowError("This ticket is closed. Change its status before replying.");
                    return;
                }

                Session["AdminSupportMessage"] = "Reply added to ticket " + SelectedTicketID + ".";
                Response.Redirect("Support.aspx?ticketId=" + SelectedTicketID, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowError("The reply could not be saved to CodeQuestDB.");
            }
        }

        protected void btnSaveStatus_Click(object sender, EventArgs e)
        {
            if (SelectedTicketID <= 0)
            {
                ShowError("Select a ticket before changing its status.");
                return;
            }

            try
            {
                if (!new SupportRepository().UpdateStatus(SelectedTicketID, ddlStatus.SelectedValue))
                {
                    ShowError("That ticket could not be found.");
                    return;
                }

                Session["AdminSupportMessage"] = "Ticket " + SelectedTicketID + " is now " + ddlStatus.SelectedValue + ".";
                Response.Redirect("Support.aspx?ticketId=" + SelectedTicketID, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowError("The ticket status could not be saved to CodeQuestDB.");
            }
        }

        private void LoadSupport()
        {
            try
            {
                SupportRepository repository = new SupportRepository();
                IList<TicketRecord> tickets = repository.GetAllTickets();
                rptTickets.DataSource = tickets;
                rptTickets.DataBind();
                pnlNoTickets.Visible = tickets.Count == 0;
                pnlSelectTicket.Visible = SelectedTicketID <= 0 && tickets.Count > 0;

                if (Session["AdminSupportMessage"] != null)
                {
                    ShowMessage(Session["AdminSupportMessage"].ToString());
                    Session.Remove("AdminSupportMessage");
                }

                if (SelectedTicketID > 0)
                {
                    TicketRecord ticket = repository.GetTicketForAdmin(SelectedTicketID);
                    if (ticket == null)
                    {
                        ShowError("That ticket could not be found.");
                    }
                    else
                    {
                        BindSelectedTicket(ticket);
                    }
                }
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowError("Support could not connect to CodeQuestDB. Confirm that the Ticket and Reply tables exist.");
            }
        }

        private void BindSelectedTicket(TicketRecord ticket)
        {
            pnlSelectedTicket.Visible = true;
            pnlSelectTicket.Visible = false;
            lblTicketID.Text = ticket.TicketID.ToString();
            lblTicketSubject.Text = Server.HtmlEncode(ticket.Subject);
            lblTicketStatus.Text = Server.HtmlEncode(ticket.Status);
            lblTicketCategory.Text = Server.HtmlEncode(ticket.Category);
            lblTicketName.Text = Server.HtmlEncode(ticket.Name);
            lblTicketEmail.Text = Server.HtmlEncode(ticket.Email);
            lblTicketDescription.Text = Server.HtmlEncode(ticket.Description).Replace("\n", "<br />");
            ddlStatus.SelectedValue = ticket.Status;
            rptReplies.DataSource = ticket.Replies;
            rptReplies.DataBind();
            bool closed = string.Equals(ticket.Status, "Closed", StringComparison.OrdinalIgnoreCase);
            pnlAdminReply.Visible = !closed;
            pnlAdminClosed.Visible = closed;
        }

        private bool TryGetAdminUserID(out int userID)
        {
            if (int.TryParse(Convert.ToString(Session["UserID"]), out userID) && userID > 0)
            {
                return true;
            }

            string email = Convert.ToString(Session["UserEmail"]);
            if (string.IsNullOrWhiteSpace(email))
            {
                userID = 0;
                return false;
            }

            UserRecord user = new UserRepository().FindByEmail(email);
            if (user == null || !string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                userID = 0;
                return false;
            }

            userID = user.UserID;
            Session["UserID"] = user.UserID;
            Session["UserPlan"] = user.Plan;
            return userID > 0;
        }

        private void ShowMessage(string message)
        {
            lblMessage.Text = Server.HtmlEncode(message);
            pnlMessage.Visible = true;
        }

        private void ShowError(string message)
        {
            lblError.Text = Server.HtmlEncode(message);
            pnlError.Visible = true;
        }
    }
}
