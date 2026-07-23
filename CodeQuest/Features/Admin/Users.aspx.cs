// Purpose: Loads searchable account records and validates administrator changes to email, role and plan.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Text;
using CodeQuest.Data.Repositories;
using CodeQuest.Models;

namespace CodeQuest.Features.Admin
{
    public partial class Users : System.Web.UI.Page
    {
        private int SelectedUserID
        {
            get
            {
                int userID;
                if (int.TryParse(Request.QueryString["userId"], out userID) && userID > 0)
                {
                    ViewState["SelectedUserID"] = userID;
                    return userID;
                }

                return int.TryParse(Convert.ToString(ViewState["SelectedUserID"]), out userID) && userID > 0
                    ? userID
                    : 0;
            }
        }

        private string SearchTerm
        {
            get { return Convert.ToString(ViewState["UserSearch"] ?? string.Empty); }
            set { ViewState["UserSearch"] = (value ?? string.Empty).Trim(); }
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
                LoadUsers();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            ResetMessages();
            SearchTerm = txtSearch.Text;
            LoadUsers();
        }

        protected void btnClearSearch_Click(object sender, EventArgs e)
        {
            ResetMessages();
            SearchTerm = string.Empty;
            txtSearch.Text = string.Empty;
            LoadUsers();
        }

        protected void btnSaveAccess_Click(object sender, EventArgs e)
        {
            ResetMessages();
            int selectedUserID = SelectedUserID;
            if (selectedUserID <= 0)
            {
                ShowError("Select a valid user before changing access.");
                return;
            }

            string role = ddlRole.SelectedValue;
            string plan = ddlPlan.SelectedValue;
            if ((!string.Equals(role, "Learner", StringComparison.Ordinal) && !string.Equals(role, "Admin", StringComparison.Ordinal)) ||
                (!string.Equals(plan, "Basic", StringComparison.Ordinal) && !string.Equals(plan, "Premium", StringComparison.Ordinal)))
            {
                ShowError("Choose a valid role and plan.");
                return;
            }

            try
            {
                UserRepository repository = new UserRepository();
                int adminUserID;
                if (!TryGetAdminUserID(repository, out adminUserID))
                {
                    ShowError("This administrator session is not linked to an admin row in dbo.User. Sign out and sign in again.");
                    return;
                }

                UserManagementRecord user = repository.GetManagedUser(selectedUserID);
                if (user == null)
                {
                    ShowError("That account could not be found.");
                    return;
                }

                bool demotingAdmin = string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(role, "Learner", StringComparison.Ordinal);

                if (selectedUserID == adminUserID && demotingAdmin)
                {
                    ShowError("You cannot remove your own administrator access.");
                    BindSelectedUser(user);
                    return;
                }

                if (demotingAdmin && repository.GetAdminCount() <= 1)
                {
                    ShowError("CodeQuest must keep at least one administrator account.");
                    BindSelectedUser(user);
                    return;
                }

                if (!repository.UpdateAccess(selectedUserID, role, plan))
                {
                    ShowError("That account could not be updated.");
                    return;
                }

                if (selectedUserID == adminUserID)
                {
                    Session["UserRole"] = role;
                    Session["UserPlan"] = plan;
                }

                ShowSuccess("Access updated for " + user.Username + ".");
                LoadUsers();
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowError("The account access could not be saved to CodeQuestDB.");
            }
        }

        protected void btnSaveEmail_Click(object sender, EventArgs e)
        {
            ResetMessages();
            int selectedUserID = SelectedUserID;
            if (selectedUserID <= 0)
            {
                ShowError("Select a valid learner before changing an email address.");
                return;
            }

            string email = (txtLearnerEmail.Text ?? string.Empty).Trim().ToLowerInvariant();
            if (!IsValidEmail(email))
            {
                ShowError("Enter a valid email address, such as learner@example.com.");
                return;
            }

            try
            {
                UserRepository repository = new UserRepository();
                int adminUserID;
                if (!TryGetAdminUserID(repository, out adminUserID))
                {
                    ShowError("This administrator session is not linked to an admin row in dbo.User. Sign out and sign in again.");
                    return;
                }

                UserManagementRecord user = repository.GetManagedUser(selectedUserID);
                if (user == null)
                {
                    ShowError("That account could not be found.");
                    return;
                }

                if (!string.Equals(user.Role, "Learner", StringComparison.OrdinalIgnoreCase))
                {
                    ShowError("Only learner email addresses can be changed from User Management.");
                    BindSelectedUser(user);
                    return;
                }

                UserRecord existing = repository.FindByEmail(email);
                if (existing != null && existing.UserID != selectedUserID)
                {
                    ShowError("That email address is already connected to another CodeQuest account.");
                    BindSelectedUser(user);
                    return;
                }

                if (string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    ShowSuccess(user.Username + " already uses that email address.");
                    BindSelectedUser(user);
                    return;
                }

                if (!repository.UpdateLearnerEmail(selectedUserID, email))
                {
                    ShowError("The learner email could not be updated. Refresh the account and try again.");
                    return;
                }

                ShowSuccess("Email updated for " + user.Username + ". The learner can now sign in and reset their password using " + email + ".");
                LoadUsers();
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException exception)
            {
                if (exception.Number == 2601 || exception.Number == 2627)
                {
                    ShowError("That email address is already connected to another CodeQuest account.");
                }
                else
                {
                    ShowError("The learner email could not be saved to CodeQuestDB.");
                }
            }
        }

        protected string GetInitial(object value)
        {
            string username = Convert.ToString(value).Trim();
            return Server.HtmlEncode(string.IsNullOrEmpty(username) ? "?" : username.Substring(0, 1).ToUpperInvariant());
        }

        private void LoadUsers()
        {
            try
            {
                UserRepository repository = new UserRepository();
                int adminUserID;
                if (!TryGetAdminUserID(repository, out adminUserID))
                {
                    ShowError("This administrator session is not linked to an admin row in dbo.User. Sign out and sign in again.");
                    rptUsers.DataSource = null;
                    rptUsers.DataBind();
                    lblUserCount.Text = "0";
                    pnlNoUsers.Visible = true;
                    pnlSelectUser.Visible = true;
                    pnlSelectedUser.Visible = false;
                    return;
                }

                IList<UserManagementRecord> users = repository.GetUsers(SearchTerm);
                rptUsers.DataSource = users;
                rptUsers.DataBind();
                lblUserCount.Text = users.Count.ToString();
                lblDirectoryTitle.Text = string.IsNullOrWhiteSpace(SearchTerm) ? "All users." : "Search results.";
                pnlNoUsers.Visible = users.Count == 0;
                txtSearch.Text = SearchTerm;

                int selectedUserID = SelectedUserID;
                pnlSelectUser.Visible = selectedUserID <= 0;
                pnlSelectedUser.Visible = false;
                if (selectedUserID > 0)
                {
                    UserManagementRecord user = repository.GetManagedUser(selectedUserID);
                    if (user == null)
                    {
                        ShowError("That account could not be found.");
                        pnlSelectUser.Visible = true;
                    }
                    else
                    {
                        BindSelectedUser(user);
                    }
                }
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowError("User management could not connect to CodeQuestDB. Confirm that dbo.User, Enrollment and Ticket exist.");
            }
        }

        private void BindSelectedUser(UserManagementRecord user)
        {
            pnlSelectUser.Visible = false;
            pnlSelectedUser.Visible = true;
            lblSelectedUserID.Text = user.UserID.ToString();
            lblSelectedUsername.Text = Server.HtmlEncode(user.Username);
            lblSelectedRole.Text = Server.HtmlEncode(user.Role);
            lblSelectedEmail.Text = Server.HtmlEncode(user.Email);
            lblSelectedGoogle.Text = string.IsNullOrWhiteSpace(user.GoogleID) ? "Not connected" : "Connected";
            lblSelectedEnrollments.Text = user.EnrollmentCount.ToString();
            lblSelectedTickets.Text = user.TicketCount.ToString();
            lblSelectedBio.Text = Server.HtmlEncode(string.IsNullOrWhiteSpace(user.Bio) ? "No biography added." : user.Bio);
            bool isLearner = string.Equals(user.Role, "Learner", StringComparison.OrdinalIgnoreCase);
            pnlLearnerEmailEditor.Visible = isLearner;
            pnlProtectedEmail.Visible = !isLearner;
            txtLearnerEmail.Text = isLearner ? user.Email : string.Empty;

            if (ddlRole.Items.FindByValue(user.Role) != null)
            {
                ddlRole.SelectedValue = user.Role;
            }

            if (ddlPlan.Items.FindByValue(user.Plan) != null)
            {
                ddlPlan.SelectedValue = user.Plan;
            }
        }

        private bool TryGetAdminUserID(UserRepository repository, out int userID)
        {
            if (int.TryParse(Convert.ToString(Session["UserID"]), out userID) && userID > 0)
            {
                UserRecord sessionUser = repository.FindByID(userID);
                if (sessionUser != null && string.Equals(sessionUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            string email = Convert.ToString(Session["UserEmail"]);
            if (string.IsNullOrWhiteSpace(email))
            {
                userID = 0;
                return false;
            }

            UserRecord user = repository.FindByEmail(email);
            if (user == null || !string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                userID = 0;
                return false;
            }

            userID = user.UserID;
            Session["UserID"] = user.UserID;
            Session["UserPlan"] = user.Plan;
            return true;
        }

        private static bool IsValidEmail(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 254)
            {
                return false;
            }

            try
            {
                MailAddress address = new MailAddress(value);
                return string.Equals(address.Address, value, StringComparison.OrdinalIgnoreCase);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private void ResetMessages()
        {
            pnlSuccess.Visible = false;
            pnlError.Visible = false;
        }

        private void ShowSuccess(string message)
        {
            lblSuccess.Text = Server.HtmlEncode(message);
            pnlSuccess.Visible = true;
            pnlError.Visible = false;
        }

        private void ShowError(string message)
        {
            lblError.Text = Server.HtmlEncode(message);
            pnlError.Visible = true;
            pnlSuccess.Visible = false;
        }
    }
}
