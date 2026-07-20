<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Tickets.aspx.cs" Inherits="CodeQuest.Features.Support.Tickets" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Contact support | CodeQuest</title>
    <link href="../../Content/codequest-home.css" rel="stylesheet" />
    <link href="../../Content/codequest-support.css" rel="stylesheet" />
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
                <a class="active" href="Tickets.aspx">Contact support</a>
            </nav>
            <div class="header-actions">
                <a class="login-link" href="../../LearnerDashboard.aspx">Dashboard</a>
                <a class="header-cta" href="../../Login.aspx?logout=1">Sign out</a>
            </div>
        </header>

        <main class="support-page">
            <section class="support-heading">
                <div>
                    <p class="eyebrow"><span></span> Contact us</p>
                    <h1>We are here to<br /><em>help you learn.</em></h1>
                    <p>Ask about your account, course access, billing or anything that is blocking your progress. Your messages stay connected to your CodeQuest account.</p>
                </div>
                <div class="support-badge"><span>Response space</span><strong>Ticket desk</strong></div>
            </section>

            <asp:Panel ID="pnlMessage" runat="server" CssClass="support-message success" Visible="false" role="status">
                <asp:Label ID="lblMessage" runat="server" />
            </asp:Panel>
            <asp:Panel ID="pnlError" runat="server" CssClass="support-message error" Visible="false" role="alert">
                <asp:Label ID="lblError" runat="server" />
            </asp:Panel>

            <div class="support-grid">
                <section class="support-card ticket-history-card">
                    <div class="support-card-heading">
                        <div><p class="section-kicker">Your tickets</p><h2>Ticket history.</h2></div>
                        <span class="section-note">Open &middot; In Progress &middot; Resolved &middot; Closed</span>
                    </div>
                    <asp:Panel ID="pnlNoTickets" runat="server" CssClass="support-empty" Visible="false">You have not contacted the support team yet.</asp:Panel>
                    <div class="ticket-list">
                        <asp:Repeater ID="rptTickets" runat="server">
                            <ItemTemplate>
                                <a class="ticket-row" href="Tickets.aspx?ticketId=<%# Eval("TicketID") %>">
                                    <div><span class="ticket-number">TICKET-<%# Eval("TicketID") %></span><strong><%# Server.HtmlEncode(Eval("Subject").ToString()) %></strong><small><%# Server.HtmlEncode(Eval("Category").ToString()) %> &middot; <%# Eval("ReplyCount") %> replies</small></div>
                                    <span class="ticket-status <%# Eval("Status").ToString().ToLowerInvariant().Replace(" ", "-") %>"><%# Eval("Status") %></span>
                                </a>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </section>

                <section class="support-card new-ticket-card">
                    <div class="support-card-heading"><div><p class="section-kicker">New request</p><h2>Start a conversation.</h2></div></div>
                    <label class="field-label" for="ddlCategory">Category</label>
                    <asp:DropDownList ID="ddlCategory" runat="server" CssClass="support-select">
                        <asp:ListItem Text="Account and sign in" Value="Account" />
                        <asp:ListItem Text="Course access" Value="Course access" />
                        <asp:ListItem Text="Billing and plans" Value="Billing" />
                        <asp:ListItem Text="Technical issue" Value="Technical" />
                        <asp:ListItem Text="Feedback" Value="Feedback" />
                    </asp:DropDownList>
                    <label class="field-label" for="txtSubject">Subject</label>
                    <asp:TextBox ID="txtSubject" runat="server" CssClass="support-input" MaxLength="200" placeholder="What do you need help with?" />
                    <label class="field-label" for="txtDescription">Message</label>
                    <asp:TextBox ID="txtDescription" runat="server" CssClass="support-input support-textarea" TextMode="MultiLine" Rows="7" placeholder="Include the details that will help our team understand the issue." />
                    <asp:Button ID="btnCreateTicket" runat="server" CssClass="primary-button support-button" Text="Create support ticket" OnClick="btnCreateTicket_Click" />
                </section>
            </div>

            <asp:Panel ID="pnlSelectedTicket" runat="server" CssClass="support-card selected-ticket-card" Visible="false">
                <div class="selected-ticket-heading">
                    <div><p class="section-kicker">Ticket-<asp:Label ID="lblTicketID" runat="server" /></p><h2><asp:Label ID="lblTicketSubject" runat="server" /></h2></div>
                    <span class="ticket-status"><asp:Label ID="lblTicketStatus" runat="server" /></span>
                </div>
                <div class="ticket-meta"><span><asp:Label ID="lblTicketCategory" runat="server" /></span><span>Created by <asp:Label ID="lblTicketName" runat="server" /></span></div>
                <div class="ticket-description"><asp:Label ID="lblTicketDescription" runat="server" /></div>
                <div class="reply-thread">
                    <asp:Repeater ID="rptReplies" runat="server">
                        <ItemTemplate>
                            <article class="reply-bubble <%# (bool)Eval("IsAdmin") ? "admin-reply" : "learner-reply" %>">
                                <div><strong><%# Server.HtmlEncode(Eval("AuthorName").ToString()) %></strong><span><%# Eval("CreatedAt", "{0:dd MMM yyyy, HH:mm}") %></span></div>
                                <p><%# Server.HtmlEncode(Eval("Message").ToString()).Replace("\n", "<br />") %></p>
                            </article>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <asp:Panel ID="pnlReply" runat="server" CssClass="reply-editor">
                    <label class="field-label" for="txtReply">Reply to this ticket</label>
                    <asp:TextBox ID="txtReply" runat="server" CssClass="support-input support-textarea" TextMode="MultiLine" Rows="4" placeholder="Add more information or respond to the support team." />
                    <asp:Button ID="btnReply" runat="server" CssClass="primary-button support-button" Text="Send reply" OnClick="btnReply_Click" />
                </asp:Panel>
                <asp:Panel ID="pnlClosedNotice" runat="server" CssClass="support-empty" Visible="false">This ticket is closed. Create a new ticket if you need more help.</asp:Panel>
            </asp:Panel>
        </main>

        <footer class="site-footer">
            <span>&copy; 2026 CodeQuest</span>
            <span>Learn &middot; Practise &middot; Build</span>
        </footer>
    </form>
</body>
</html>
