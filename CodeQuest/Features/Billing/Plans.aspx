<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Plans.aspx.cs" Inherits="CodeQuest.Features.Billing.Plans" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Plans | CodeQuest</title>
    <link href="../../Content/codequest-home.css" rel="stylesheet" />
    <link href="../../Content/codequest-billing.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <header class="site-header">
            <a class="brand" href="../../Guest.aspx" aria-label="CodeQuest home">
                <img class="brand-logo" src="../../Content/Images/CodeQuest_logo.png" alt="CodeQuest" />
            </a>
            <nav class="main-nav" aria-label="Learner navigation">
                <a href="../../LearnerDashboard.aspx">Dashboard</a>
                <a href="../Public/Courses.aspx">Courses</a>
                <a href="../../LearnerDashboard.aspx#myLearning">My learning</a>
                <a class="active" href="Plans.aspx">Plans</a>
                <a href="../Support/Tickets.aspx">Support</a>
            </nav>
            <div class="header-actions">
                <a class="login-link" href="../../LearnerDashboard.aspx">Dashboard</a>
                <a class="header-cta" href="../../Login.aspx?logout=1">Sign out</a>
            </div>
        </header>

        <main class="billing-page">
            <section class="billing-heading">
                <div>
                    <p class="eyebrow"><span></span> Simple subscription</p>
                    <h1>Choose how far<br />you want to <em>go.</em></h1>
                    <p>Start with the free beginner path or unlock every CodeQuest course with Premium.</p>
                </div>
                <div class="current-plan-badge"><span>Current plan</span><strong><asp:Label ID="lblCurrentPlan" runat="server" Text="Basic" /></strong></div>
            </section>

            <asp:Panel ID="pnlError" runat="server" CssClass="billing-message error" Visible="false" role="alert">
                <asp:Label ID="lblError" runat="server" />
            </asp:Panel>
            <asp:Panel ID="pnlSuccess" runat="server" CssClass="billing-message success" Visible="false" role="status">
                <asp:Label ID="lblSuccess" runat="server" />
            </asp:Panel>

            <section class="plans-grid" aria-label="Subscription plans">
                <article class="plan-card basic-plan">
                    <div class="plan-card-topline"><span>PLAN 01</span><span class="plan-tag">Free forever</span></div>
                    <h2>Basic</h2>
                    <p class="plan-price"><strong>RM0</strong><span> forever</span></p>
                    <p class="plan-description">A friendly starting point for learning the foundations of web development.</p>
                    <ul class="plan-features">
                        <li>Beginner courses</li>
                        <li>All public tutorials and exercises</li>
                        <li>Chapter quizzes and saved progress</li>
                    </ul>
                    <span class="plan-current"><asp:Label ID="lblBasicStatus" runat="server" Text="Available on your account" /></span>
                </article>

                <article class="plan-card premium-plan">
                    <div class="plan-card-topline"><span>PLAN 02</span><span class="plan-tag premium-tag">Recommended</span></div>
                    <h2>Premium</h2>
                    <p class="plan-price"><strong>RM29</strong><span> / month</span></p>
                    <p class="plan-description">Move beyond the basics with the complete CodeQuest learning path.</p>
                    <ul class="plan-features">
                        <li>Beginner, Intermediate and Advanced courses</li>
                        <li>All public tutorials, exercises and quizzes</li>
                        <li>AI learning assistant when it is enabled</li>
                    </ul>
                    <asp:Panel ID="pnlPremiumActive" runat="server" CssClass="plan-active-note" Visible="false">
                        <strong>Premium is active.</strong><span>Your learning path is fully unlocked.</span>
                    </asp:Panel>
                    <asp:Panel ID="pnlPremiumUpgrade" runat="server" CssClass="premium-upgrade-panel">
                        <asp:Button ID="btnUpgrade" runat="server" CssClass="primary-button upgrade-button" Text="Activate Premium — RM29/month" OnClick="btnUpgrade_Click" />
                        <small>Prototype checkout: no real charge and no card details are stored.</small>
                    </asp:Panel>
                </article>
            </section>

            <section class="checkout-note">
                <div><p class="section-kicker">Demo checkout</p><h2>One clear step to unlock more.</h2></div>
                <p>When you activate Premium, CodeQuest creates an Active subscription and a Completed demo payment in the database, then returns you to My learning.</p>
            </section>

            <section class="payment-history">
                <div class="section-heading-row">
                    <div><p class="section-kicker">Billing history</p><h2>Your payments.</h2></div>
                    <span class="section-note">Stored in CodeQuestDB</span>
                </div>
                <asp:Panel ID="pnlNoPayments" runat="server" CssClass="billing-message" Visible="false">No subscription payments have been recorded yet.</asp:Panel>
                <div class="payment-list">
                    <asp:Repeater ID="rptPayments" runat="server">
                        <ItemTemplate>
                            <article class="payment-row">
                                <div><strong><%# Eval("Status") %></strong><span><%# Eval("PaidAt", "{0:dd MMM yyyy, HH:mm}") %></span></div>
                                <div><span class="payment-reference"><%# Server.HtmlEncode(Convert.ToString(Eval("TransactionReference"))) %></span><strong>RM<%# Eval("Amount", "{0:0.00}") %></strong></div>
                            </article>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </section>
        </main>

        <footer class="site-footer">
            <span>&copy; 2026 CodeQuest</span>
            <span>Learn &middot; Practise &middot; Build</span>
        </footer>
    </form>
</body>
</html>
