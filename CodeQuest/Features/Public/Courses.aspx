<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Courses.aspx.cs" Inherits="CodeQuest.Features.Public.Courses" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Courses | CodeQuest</title>
    <link href="../../Content/codequest-home.css" rel="stylesheet" />
    <link href="../../Content/codequest-courses.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <header class="site-header">
            <a class="brand" href="../../Guest.aspx" aria-label="CodeQuest home">
                <img class="brand-logo" src="../../Content/Images/CodeQuest_logo.png" alt="CodeQuest" />
            </a>
            <nav class="main-nav" aria-label="Main navigation">
                <a href="../../Guest.aspx">Home</a>
                <a class="active" href="Courses.aspx">Courses</a>
                <a href="../../Guest.aspx#tutorials">Tutorials</a>
                <a href="../../Guest.aspx#about">About</a>
            </nav>
            <asp:Panel ID="pnlGuestActions" runat="server" CssClass="header-actions">
                <a class="login-link" href="../../Login.aspx">Login</a>
                <a class="header-cta" href="../../Register.aspx">Get Started</a>
            </asp:Panel>
            <asp:Panel ID="pnlSignedInActions" runat="server" CssClass="header-actions" Visible="false">
                <a class="login-link" href="../../LearnerDashboard.aspx">Dashboard</a>
                <a class="header-cta" href="../../LearnerDashboard.aspx#myLearning">My learning</a>
            </asp:Panel>
        </header>

        <main class="catalogue-page">
            <section class="catalogue-hero">
                <p class="eyebrow"><span></span> CodeQuest catalogue</p>
                <h1>Find your next <em>skill.</em></h1>
                <p>Browse the courses managed by CodeQuest administrators. Guests can preview the catalogue; log in to enrol and save progress.</p>
            </section>

            <asp:Panel ID="pnlError" runat="server" CssClass="catalogue-message error" Visible="false" role="alert">
                <asp:Label ID="lblError" runat="server" />
            </asp:Panel>

            <asp:Panel ID="pnlEmpty" runat="server" CssClass="catalogue-message" Visible="false">
                No courses have been published yet. An administrator can add the first course from the Admin feature.
            </asp:Panel>

            <section class="catalogue-grid" aria-label="Available courses">
                <asp:Repeater ID="rptCourses" runat="server">
                    <ItemTemplate>
                        <article class="catalogue-card">
                            <div class="catalogue-card-top">
                                <span class="catalogue-code">COURSE-<%# Eval("CourseID") %></span>
                                <span class="catalogue-level"><%# Eval("Difficulty") %></span>
                            </div>
                            <h2><%# Server.HtmlEncode(Eval("Title").ToString()) %></h2>
                            <p><%# Server.HtmlEncode(Eval("Description") == null ? "Build practical web development skills with guided lessons." : Eval("Description").ToString()) %></p>
                            <div class="catalogue-card-footer">
                                <span>Course owner ID: <%# Eval("OwnerUserID") %></span>
                                <a href="<%# Eval("ActionUrl") %>"><%# Eval("ActionText") %> &rarr;</a>
                            </div>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>
            </section>
        </main>

        <footer class="site-footer">
            <span>&copy; 2026 CodeQuest</span>
            <span>Learn &middot; Practise &middot; Build</span>
        </footer>
    </form>
</body>
</html>
