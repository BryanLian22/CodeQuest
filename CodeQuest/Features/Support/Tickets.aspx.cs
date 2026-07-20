using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using CodeQuest.Data.Repositories;
using CodeQuest.Models;

namespace CodeQuest.Features.Support
{
    public partial class Tickets : System.Web.UI.Page
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
            if (!string.Equals(Convert.ToString(Session["UserRole"]), "Learner", StringComparison.OrdinalIgnoreCase))
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

        protected void btnCreateTicket_Click(object sender, EventArgs e)
        {
            int userID;
            if (!TryGetUserID(out userID))
            {
                ShowError("Your sign-in is not linked to a database learner account.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSubject.Text) || string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                ShowError("Enter both a subject and a message before creating a ticket.");
                return;
            }

            try
            {
                int ticketID = new SupportRepository().CreateTicket(
                    userID,
                    Convert.ToString(Session["DisplayName"] ?? "CodeQuest learner"),
                    Convert.ToString(Session["UserEmail"] ?? string.Empty),
                    ddlCategory.SelectedValue,
                    txtSubject.Text,
                    txtDescription.Text);

                Session["SupportMessage"] = "Ticket " + ticketID + " was created. Our support team can now reply to you.";
                Response.Redirect("Tickets.aspx?ticketId=" + ticketID, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowError("Your ticket could not be saved to CodeQuestDB.");
            }
        }

        protected void btnReply_Click(object sender, EventArgs e)
        {
            int userID;
            if (!TryGetUserID(out userID) || SelectedTicketID <= 0)
            {
                ShowError("Select a valid ticket before sending a reply.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtReply.Text))
            {
                ShowError("Enter a reply before sending it.");
                return;
            }

            try
            {
                if (!new SupportRepository().AddReply(SelectedTicketID, userID, txtReply.Text, false))
                {
                    ShowError("This ticket is closed or no longer belongs to your account.");
                    return;
                }

                Session["SupportMessage"] = "Your reply was added to ticket " + SelectedTicketID + ".";
                Response.Redirect("Tickets.aspx?ticketId=" + SelectedTicketID, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowError("Your reply could not be saved to CodeQuestDB.");
            }
        }

        private void LoadSupport()
        {
            int userID;
            if (!TryGetUserID(out userID))
            {
                ShowError("Register and sign in with a database learner account before contacting support.");
                return;
            }

            try
            {
                SupportRepository repository = new SupportRepository();
                IList<TicketRecord> tickets = repository.GetTicketsForUser(userID);
                rptTickets.DataSource = tickets;
                rptTickets.DataBind();
                pnlNoTickets.Visible = tickets.Count == 0;

                if (Session["SupportMessage"] != null)
                {
                    ShowMessage(Session["SupportMessage"].ToString());
                    Session.Remove("SupportMessage");
                }

                if (SelectedTicketID > 0)
                {
                    TicketRecord ticket = repository.GetTicketForUser(SelectedTicketID, userID);
                    if (ticket == null)
                    {
                        ShowError("That ticket could not be found in your account.");
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
            lblTicketID.Text = ticket.TicketID.ToString();
            lblTicketSubject.Text = Server.HtmlEncode(ticket.Subject);
            lblTicketCategory.Text = Server.HtmlEncode(ticket.Category);
            lblTicketName.Text = Server.HtmlEncode(ticket.Name);
            lblTicketStatus.Text = Server.HtmlEncode(ticket.Status);
            lblTicketDescription.Text = Server.HtmlEncode(ticket.Description).Replace("\n", "<br />");
            rptReplies.DataSource = ticket.Replies;
            rptReplies.DataBind();
            bool closed = string.Equals(ticket.Status, "Closed", StringComparison.OrdinalIgnoreCase);
            pnlReply.Visible = !closed;
            pnlClosedNotice.Visible = closed;
        }

        private bool TryGetUserID(out int userID)
        {
            return int.TryParse(Convert.ToString(Session["UserID"]), out userID) && userID > 0;
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
