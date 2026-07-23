<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminDashboard.aspx.cs" Inherits="CodeQuest.AdminDashboard" %>
<!-- Page purpose: Shows administrators the database-backed content overview and management navigation. -->

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Admin Dashboard | CodeQuest</title>
    <link href="Content/codequest-home.css?v=50" rel="stylesheet" />
    <link href="Content/codequest-admin.css?v=50" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <header class="site-header">
            <a class="brand" href="Guest.aspx" aria-label="CodeQuest home">
                <img class="brand-logo" src="Content/Images/CodeQuest_logo.png" alt="CodeQuest" />
            </a>
            <nav class="main-nav" aria-label="Admin navigation">
                <a class="active" href="AdminDashboard.aspx">Overview</a>
                <a href="Features/Admin/Content.aspx">Content studio</a>
                <a href="Features/Admin/Lessons.aspx">Lesson library</a>
                <a href="Features/Admin/Users.aspx">Users</a>
                <a href="Features/Admin/Support.aspx">Support tickets</a>
                <a href="Features/Public/Courses.aspx">Preview courses</a>
                <a href="Features/Public/Tutorials.aspx">Preview tutorials</a>
                <a href="Guest.aspx#about">About</a>
            </nav>
            <div class="header-actions">
                <a class="login-link" href="Guest.aspx">View site</a>
                <a class="header-cta" href="Login.aspx?logout=1">Sign out</a>
            </div>
        </header>

        <main class="admin-page">
            <section class="admin-heading">
                <div>
                    <p class="eyebrow"><span></span> Admin workspace</p>
                    <h1>Shape the learning library.</h1>
                    <p>Review the content connected to the CodeQuest learning catalogue. Publishing controls will build on this overview.</p>
                </div>
                <div class="admin-badge"><span>Signed in as</span><strong><asp:Label ID="lblAdminName" runat="server" /></strong></div>
            </section>

            <asp:Panel ID="pnlMessage" runat="server" CssClass="admin-message" Visible="false" role="alert">
                <asp:Label ID="lblMessage" runat="server" />
            </asp:Panel>

            <section class="admin-stats" aria-label="Content summary">
                <article><span>Courses</span><strong><asp:Label ID="lblCourses" runat="server" Text="0" /></strong></article>
                <article><span>Modules</span><strong><asp:Label ID="lblModules" runat="server" Text="0" /></strong></article>
                <article><span>Chapters</span><strong><asp:Label ID="lblChapters" runat="server" Text="0" /></strong></article>
                <article><span>Published tutorials</span><strong><asp:Label ID="lblTutorials" runat="server" Text="0" /></strong></article>
                <article><span>Exercises</span><strong><asp:Label ID="lblExercises" runat="server" Text="0" /></strong></article>
                <article><span>Quizzes</span><strong><asp:Label ID="lblQuizzes" runat="server" Text="0" /></strong></article>
            </section>

            <section class="admin-section" aria-labelledby="coursesTitle">
                <div class="section-heading-row">
                    <div>
                        <p class="section-kicker">Course catalogue</p>
                        <h2 id="coursesTitle">Recent courses.</h2>
                    </div>
                    <span class="coming-soon">Create and edit next</span>
                </div>

                <asp:Panel ID="pnlEmpty" runat="server" CssClass="admin-message" Visible="false">
                    No courses have been created yet.
                </asp:Panel>

                <div class="admin-course-grid">
                    <asp:Repeater ID="rptCourses" runat="server">
                        <ItemTemplate>
                            <article class="admin-course-card">
                                <div class="course-card-topline"><span>COURSE-<%# Eval("CourseID") %></span><span><%# Server.HtmlEncode(Eval("Difficulty").ToString()) %></span></div>
                                <h3><%# Server.HtmlEncode(Eval("Title").ToString()) %></h3>
                                <p>Owner: <%# Server.HtmlEncode(Eval("OwnerName").ToString()) %></p>
                                <footer><span><%# Eval("ModuleCount") %> modules</span><a href="Features/Learner/Course.aspx?courseId=<%# Eval("CourseID") %>">Test course &rarr;</a></footer>
                            </article>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </section>
        </main>

        <footer class="site-footer">
            <span>&copy; 2026 CodeQuest</span>
            <span>Admin &middot; Curate &middot; Publish</span>
        </footer>
    </form>
    <script src="Content/codequest-responsive-nav.js?v=50"></script>
</body>
</html>
