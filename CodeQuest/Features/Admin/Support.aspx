<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Support.aspx.cs" Inherits="CodeQuest.Features.Admin.Support" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Support tickets | CodeQuest</title>
    <link href="../../Content/codequest-home.css" rel="stylesheet" />
    <link href="../../Content/codequest-admin.css" rel="stylesheet" />
    <link href="../../Content/codequest-support.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <header class="site-header">
            <a class="brand" href="../../Guest.aspx" aria-label="CodeQuest home">
                <img class="brand-logo" src="../../Content/Images/CodeQuest_logo.png" alt="CodeQuest" />
            </a>
            <nav class="main-nav" aria-label="Admin navigation">
                <a href="../../AdminDashboard.aspx">Overview</a>
                <a href="Content.aspx">Content studio</a>
                <a href="Lessons.aspx">Lesson library</a>
                <a href="Users.aspx">Users</a>
                <a class="active" href="Support.aspx">Support tickets</a>
                <a href="../Public/Courses.aspx">Preview courses</a>
                <a href="../Public/Tutorials.aspx">Preview tutorials</a>
            </nav>
            <div class="header-actions">
                <a class="login-link" href="../../Guest.aspx">View site</a>
                <a class="header-cta" href="../../Login.aspx?logout=1">Sign out</a>
            </div>
        </header>

        <main class="support-page admin-support-page">
            <section class="support-heading">
                <div>
                    <p class="eyebrow"><span></span> Admin support desk</p>
                    <h1>Keep learners<br /><em>moving forward.</em></h1>
                    <p>Review learner questions, reply with guidance and keep every ticket moving through a clear status.</p>
                </div>
                <div class="support-badge"><span>Ticket statuses</span><strong>Open &middot; In Progress</strong></div>
            </section>

            <asp:Panel ID="pnlMessage" runat="server" CssClass="support-message success" Visible="false" role="status">
                <asp:Label ID="lblMessage" runat="server" />
            </asp:Panel>
            <asp:Panel ID="pnlError" runat="server" CssClass="support-message error" Visible="false" role="alert">
                <asp:Label ID="lblError" runat="server" />
            </asp:Panel>

            <div class="support-grid admin-support-grid">
                <section class="support-card ticket-history-card">
                    <div class="support-card-heading"><div><p class="section-kicker">Inbox</p><h2>All tickets.</h2></div><span class="section-note">Newest first</span></div>
                    <asp:Panel ID="pnlNoTickets" runat="server" CssClass="support-empty" Visible="false">There are no learner tickets yet.</asp:Panel>
                    <div class="ticket-list">
                        <asp:Repeater ID="rptTickets" runat="server">
                            <ItemTemplate>
                                <a class="ticket-row" href="Support.aspx?ticketId=<%# Eval("TicketID") %>">
                                    <div><span class="ticket-number">TICKET-<%# Eval("TicketID") %> &middot; <%# Server.HtmlEncode(Eval("Name").ToString()) %></span><strong><%# Server.HtmlEncode(Eval("Subject").ToString()) %></strong><small><%# Server.HtmlEncode(Eval("Category").ToString()) %> &middot; <%# Eval("ReplyCount") %> replies</small></div>
                                    <span class="ticket-status <%# Eval("Status").ToString().ToLowerInvariant().Replace(" ", "-") %>"><%# Eval("Status") %></span>
                                </a>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </section>

                <asp:Panel ID="pnlSelectedTicket" runat="server" CssClass="support-card selected-ticket-card admin-ticket-card" Visible="false">
                    <div class="selected-ticket-heading">
                        <div><p class="section-kicker">Ticket-<asp:Label ID="lblTicketID" runat="server" /></p><h2><asp:Label ID="lblTicketSubject" runat="server" /></h2></div>
                        <span class="ticket-status"><asp:Label ID="lblTicketStatus" runat="server" /></span>
                    </div>
                    <div class="ticket-meta"><span><asp:Label ID="lblTicketCategory" runat="server" /></span><span><asp:Label ID="lblTicketName" runat="server" /> &middot; <asp:Label ID="lblTicketEmail" runat="server" /></span></div>
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
                    <asp:Panel ID="pnlAdminReply" runat="server" CssClass="reply-editor">
                        <label class="field-label" for="txtAdminReply">Reply to learner</label>
                        <asp:TextBox ID="txtAdminReply" runat="server" CssClass="support-input support-textarea" TextMode="MultiLine" Rows="4" placeholder="Write a helpful response." />
                        <asp:Button ID="btnAdminReply" runat="server" CssClass="primary-button support-button" Text="Send reply" OnClick="btnAdminReply_Click" />
                    </asp:Panel>
                    <asp:Panel ID="pnlAdminClosed" runat="server" CssClass="support-empty" Visible="false">This ticket is closed. Change the status to reopen it before replying.</asp:Panel>
                    <div class="status-editor">
                        <label class="field-label" for="ddlStatus">Ticket status</label>
                        <div class="status-editor-row">
                            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="support-select">
                                <asp:ListItem Text="Open" Value="Open" />
                                <asp:ListItem Text="In Progress" Value="In Progress" />
                                <asp:ListItem Text="Resolved" Value="Resolved" />
                                <asp:ListItem Text="Closed" Value="Closed" />
                            </asp:DropDownList>
                            <asp:Button ID="btnSaveStatus" runat="server" CssClass="secondary-button support-button" Text="Save status" OnClick="btnSaveStatus_Click" />
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlSelectTicket" runat="server" CssClass="support-card support-empty select-ticket-panel" Visible="false">Select a ticket to read the conversation and respond.</asp:Panel>
            </div>
        </main>

        <footer class="site-footer">
            <span>&copy; 2026 CodeQuest</span>
            <span>Admin &middot; Listen &middot; Respond</span>
        </footer>
    </form>
</body>
</html>
