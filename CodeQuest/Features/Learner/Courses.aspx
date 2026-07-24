<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Courses.aspx.cs" Inherits="CodeQuest.Features.Learner.Courses" %>
<!-- Page purpose: Shows the signed-in learner catalogue with enrolment-aware actions and learner navigation. -->

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Learner Courses | CodeQuest</title>
    <link href="../../Content/codequest-home.css?v=50" rel="stylesheet" />
    <link href="../../Content/codequest-courses.css?v=51" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <header class="site-header">
            <a class="brand" href="../../Guest.aspx" aria-label="CodeQuest home">
                <img class="brand-logo" src="../../Content/Images/CodeQuest_logo.png" alt="CodeQuest" />
            </a>
            <nav class="main-nav" aria-label="Learner navigation">
                <a href="../../LearnerDashboard.aspx">Dashboard</a>
                <a class="active" href="Courses.aspx">Courses</a>
                <a href="../../LearnerDashboard.aspx#myLearning">My learning</a>
                <a href="../AI/Assistant.aspx">AI assistant</a>
                <a href="Profile.aspx">Profile</a>
                <a href="../Support/Tickets.aspx">Support</a>
            </nav>
            <div class="header-actions">
                <a class="login-link" href="../../Guest.aspx">Home</a>
                <a class="header-cta" href="../../Login.aspx?logout=1">Sign out</a>
            </div>
        </header>

        <main class="catalogue-page">
            <section class="catalogue-hero">
                <p class="eyebrow"><span></span> Learner catalogue</p>
                <h1>Choose your next <em>challenge.</em></h1>
                <p>Explore every course available to your account. Enrol in a new path, continue an active course or review one you have completed.</p>
            </section>

            <asp:Panel ID="pnlError" runat="server" CssClass="catalogue-message error" Visible="false" role="alert">
                <asp:Label ID="lblError" runat="server" />
            </asp:Panel>

            <asp:Panel ID="pnlEmpty" runat="server" CssClass="catalogue-message" Visible="false">
                No courses are available yet. An administrator can add the first course from the Admin workspace.
            </asp:Panel>

            <section class="catalogue-grid" aria-label="Learner courses">
                <asp:Repeater ID="rptCourses" runat="server">
                    <ItemTemplate>
                        <article class="catalogue-card">
                            <div class="catalogue-card-top">
                                <span class="catalogue-code">COURSE-<%# Eval("CourseID") %></span>
                                <span class="<%# GetDifficultyCss(Eval("Difficulty")) %>"><%# Eval("Difficulty") %></span>
                            </div>
                            <h2><%# Server.HtmlEncode(Eval("Title").ToString()) %></h2>
                            <p><%# Server.HtmlEncode(Eval("Description") == null ? "Build practical web development skills with guided lessons." : Eval("Description").ToString()) %></p>
                            <div class="catalogue-card-footer">
                                <span>CodeQuest learning path</span>
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
    <script src="../../Content/codequest-responsive-nav.js?v=50"></script>
</body>
</html>
