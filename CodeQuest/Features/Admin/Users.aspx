<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Users.aspx.cs" Inherits="CodeQuest.Features.Admin.Users" %>
<!-- Page purpose: Lets administrators find users and manage learner email, role and plan details. -->

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>User management | CodeQuest</title>
    <link href="../../Content/codequest-home.css?v=50" rel="stylesheet" />
    <link href="../../Content/codequest-admin.css?v=50" rel="stylesheet" />
    <link href="../../Content/codequest-admin-users.css?v=43" rel="stylesheet" />
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
                <a class="active" href="Users.aspx">Users</a>
                <a href="Support.aspx">Support tickets</a>
                <a href="../Public/Courses.aspx">Preview courses</a>
                <a href="../Public/Tutorials.aspx">Preview tutorials</a>
            </nav>
            <div class="header-actions">
                <a class="login-link" href="../../Guest.aspx">View site</a>
                <a class="header-cta" href="../../Login.aspx?logout=1">Sign out</a>
            </div>
        </header>

        <main class="users-page">
            <section class="users-heading">
                <div>
                    <p class="eyebrow"><span></span> User management</p>
                    <h1>Guide every account.</h1>
                    <p>Find learners and administrators, review their activity totals, and safely manage role and plan access.</p>
                </div>
                <div class="users-badge"><span>Admin control</span><strong>Safe access</strong><small>No passwords exposed</small></div>
            </section>

            <asp:Panel ID="pnlSuccess" runat="server" CssClass="users-message success" Visible="false" role="status">
                <asp:Label ID="lblSuccess" runat="server" />
            </asp:Panel>
            <asp:Panel ID="pnlError" runat="server" CssClass="users-message error" Visible="false" role="alert">
                <asp:Label ID="lblError" runat="server" />
            </asp:Panel>

            <section class="search-card">
                <div>
                    <p class="section-kicker">Directory</p>
                    <h2>Search accounts.</h2>
                </div>
                <div class="search-controls">
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="users-input" MaxLength="254" placeholder="Search username or email" />
                    <asp:Button ID="btnSearch" runat="server" CssClass="users-button search-button" Text="Search" OnClick="btnSearch_Click" />
                    <asp:Button ID="btnClearSearch" runat="server" CssClass="secondary-button" Text="Clear" OnClick="btnClearSearch_Click" />
                </div>
            </section>

            <div class="users-layout">
                <section class="directory-card">
                    <div class="directory-heading">
                        <div><p class="section-kicker">Accounts</p><h2><asp:Label ID="lblDirectoryTitle" runat="server" Text="All users." /></h2></div>
                        <span><asp:Label ID="lblUserCount" runat="server" Text="0" /> results</span>
                    </div>
                    <asp:Panel ID="pnlNoUsers" runat="server" CssClass="empty-state" Visible="false">No accounts match that search.</asp:Panel>
                    <div class="user-list">
                        <asp:Repeater ID="rptUsers" runat="server">
                            <ItemTemplate>
                                <a class="user-row" href="Users.aspx?userId=<%# Eval("UserID") %>">
                                    <div class="user-avatar"><%# GetInitial(Eval("Username")) %></div>
                                    <div class="user-summary">
                                        <strong><%# Server.HtmlEncode(Eval("Username").ToString()) %></strong>
                                        <span><%# Server.HtmlEncode(Eval("Email").ToString()) %></span>
                                        <small>USER-<%# Eval("UserID") %> &middot; <%# Eval("EnrollmentCount") %> enrolments &middot; <%# Eval("TicketCount") %> tickets</small>
                                    </div>
                                    <div class="access-tags"><span class="role-tag"><%# Eval("Role") %></span><span class="plan-tag"><%# Eval("Plan") %></span></div>
                                </a>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </section>

                <asp:Panel ID="pnlSelectUser" runat="server" CssClass="editor-card empty-editor">
                    <p class="section-kicker">Account editor</p>
                    <h2>Select a user.</h2>
                    <p>Choose an account from the directory to review its safe profile information and manage access.</p>
                </asp:Panel>

                <asp:Panel ID="pnlSelectedUser" runat="server" CssClass="editor-card" Visible="false">
                    <div class="selected-user-heading">
                        <div><p class="section-kicker">USER-<asp:Label ID="lblSelectedUserID" runat="server" /></p><h2><asp:Label ID="lblSelectedUsername" runat="server" /></h2></div>
                        <span class="selected-role"><asp:Label ID="lblSelectedRole" runat="server" /></span>
                    </div>

                    <div class="detail-list">
                        <div><span>Email</span><strong><asp:Label ID="lblSelectedEmail" runat="server" /></strong></div>
                        <div><span>Google account</span><strong><asp:Label ID="lblSelectedGoogle" runat="server" /></strong></div>
                        <div><span>Enrolments</span><strong><asp:Label ID="lblSelectedEnrollments" runat="server" /></strong></div>
                        <div><span>Support tickets</span><strong><asp:Label ID="lblSelectedTickets" runat="server" /></strong></div>
                    </div>

                    <div class="bio-summary">
                        <span>Biography</span>
                        <p><asp:Label ID="lblSelectedBio" runat="server" /></p>
                    </div>

                    <asp:Panel ID="pnlLearnerEmailEditor" runat="server" CssClass="email-editor" Visible="false">
                        <p class="section-kicker">Learner email</p>
                        <label class="field-label" for="txtLearnerEmail">Email address</label>
                        <asp:TextBox ID="txtLearnerEmail" runat="server" CssClass="users-input" TextMode="Email"
                            MaxLength="254" autocomplete="off" placeholder="learner@example.com" />
                        <p class="email-note">This becomes the learner's email/password login and password-reset destination. A connected Google account remains linked.</p>
                        <asp:Button ID="btnSaveEmail" runat="server" CssClass="users-button" Text="Update learner email" OnClick="btnSaveEmail_Click" />
                    </asp:Panel>

                    <asp:Panel ID="pnlProtectedEmail" runat="server" CssClass="protected-email-note" Visible="false">
                        Administrator email addresses are protected here. This control is available only when the selected account is a learner.
                    </asp:Panel>

                    <div class="access-editor">
                        <p class="section-kicker">Access controls</p>
                        <label class="field-label" for="ddlRole">Role</label>
                        <asp:DropDownList ID="ddlRole" runat="server" CssClass="users-select">
                            <asp:ListItem Text="Learner" Value="Learner" />
                            <asp:ListItem Text="Administrator" Value="Admin" />
                        </asp:DropDownList>

                        <label class="field-label" for="ddlPlan">Plan</label>
                        <asp:DropDownList ID="ddlPlan" runat="server" CssClass="users-select">
                            <asp:ListItem Text="Basic" Value="Basic" />
                            <asp:ListItem Text="Premium" Value="Premium" />
                        </asp:DropDownList>

                        <p class="access-note">Basic learners access beginner courses. Premium learners can access all course difficulties. An administrator cannot remove their own admin access, and the final admin is protected.</p>
                        <asp:Button ID="btnSaveAccess" runat="server" CssClass="users-button" Text="Save role and plan" OnClick="btnSaveAccess_Click" />
                    </div>
                </asp:Panel>
            </div>
        </main>

        <footer class="site-footer">
            <span>&copy; 2026 CodeQuest</span>
            <span>Admin &middot; Accounts &middot; Access</span>
        </footer>
    </form>
    <script src="../../Content/codequest-responsive-nav.js?v=50"></script>
</body>
</html>
