using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using CodeQuest.Data.Repositories;
using CodeQuest.Models;

namespace CodeQuest.Features.Billing
{
    public partial class Plans : System.Web.UI.Page
    {
        protected override void OnPreInit(EventArgs e)
        {
            Response.ContentEncoding = Encoding.UTF8;
            Response.Charset = "utf-8";
            base.OnPreInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            string role = Convert.ToString(Session["UserRole"]);
            if (string.IsNullOrWhiteSpace(role))
            {
                Session["ReturnUrl"] = Request.RawUrl;
                Response.Redirect("../../Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!string.Equals(role, "Learner", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("../../AdminDashboard.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadBilling();
            }
        }

        private void LoadBilling()
        {
            int userID;
            if (!TryGetUserID(out userID))
            {
                ShowError("Register and sign in with a database learner account before managing a plan.");
                return;
            }

            try
            {
                BillingRepository repository = new BillingRepository();
                UserRecord account = new UserRepository().FindByID(userID);
                if (account == null)
                {
                    ShowError("Your learner account could not be found in CodeQuestDB.");
                    return;
                }

                // dbo.User is the source of truth. Subscription rows are billing
                // history and must not make an administrator's Basic change look
                // like an active Premium plan.
                string plan = account.Plan;
                if (string.IsNullOrWhiteSpace(plan))
                {
                    plan = "Basic";
                }

                Session["UserPlan"] = plan;
                lblCurrentPlan.Text = Server.HtmlEncode(plan);
                bool premium = string.Equals(plan, "Premium", StringComparison.OrdinalIgnoreCase);
                pnlPremiumActive.Visible = premium;
                pnlPremiumUpgrade.Visible = !premium;
                lblBasicStatus.Text = premium ? "Your account has moved to Premium" : "Included with your account";

                IList<PaymentRecord> payments = repository.GetPaymentHistory(userID);
                rptPayments.DataSource = payments;
                rptPayments.DataBind();
                pnlNoPayments.Visible = payments.Count == 0;
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowError("Plans could not connect to CodeQuestDB. Make sure the subscription and payment tables exist.");
            }
        }

        protected void btnUpgrade_Click(object sender, EventArgs e)
        {
            int userID;
            if (!TryGetUserID(out userID))
            {
                ShowError("Register and sign in with a database learner account before upgrading.");
                return;
            }

            string transactionReference = "CQ-DEMO-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "-" + userID;
            try
            {
                PremiumPurchaseResult result = new BillingRepository().ActivatePremium(userID, transactionReference);
                Session["UserPlan"] = "Premium";
                Session["DashboardMessage"] = result.AlreadyPremium
                    ? "Your Premium plan is already active."
                    : "Premium is now active. Your account was upgraded successfully.";
                Response.Redirect("../../LearnerDashboard.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (ConfigurationErrorsException)
            {
                ShowError("The database connection is not configured. Add CodeQuestDb to Web.config.");
            }
            catch (SqlException)
            {
                ShowError("The Premium upgrade could not be saved. Check that the Subscription and Payment tables are available, then try again.");
            }
            catch (InvalidOperationException exception)
            {
                ShowError(exception.Message);
            }
        }

        private bool TryGetUserID(out int userID)
        {
            return int.TryParse(Convert.ToString(Session["UserID"]), out userID) && userID > 0;
        }

        private void ShowError(string message)
        {
            lblError.Text = Server.HtmlEncode(message);
            pnlError.Visible = true;
            pnlSuccess.Visible = false;
        }
    }
}
