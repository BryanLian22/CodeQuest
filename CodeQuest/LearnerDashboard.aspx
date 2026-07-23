<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LearnerDashboard.aspx.cs" Inherits="CodeQuest.LearnerDashboard" %>
<!-- Page purpose: Summarizes the learner plan, enrolments, progress, streak and quiz results. -->

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Learner Dashboard | CodeQuest</title>
    <link href="Content/codequest-auth.css?v=50" rel="stylesheet" />
    <link href="Content/codequest-learner.css?v=50" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <header class="site-header">
            <a class="brand" href="Guest.aspx" aria-label="CodeQuest home">
                <img class="brand-logo" src="Content/Images/CodeQuest_logo.png" alt="CodeQuest" />
            </a>
            <nav class="main-nav" aria-label="Learner navigation">
                <a class="active" href="LearnerDashboard.aspx">Dashboard</a>
                <a href="Features/Learner/Courses.aspx">Courses</a>
                <a href="#myLearning">My learning</a>
                <a href="Features/AI/Assistant.aspx">AI assistant</a>
                <a href="Features/Learner/Profile.aspx">Profile</a>
                <a href="Features/Support/Tickets.aspx">Support</a>
            </nav>
            <div class="header-actions">
                <a class="login-link" href="Guest.aspx">Home</a>
                <a class="header-cta" href="Login.aspx?logout=1">Sign out</a>
            </div>
        </header>

        <main class="learner-page">
            <section class="learner-heading">
                <div>
                    <p class="eyebrow"><span></span> Learner space</p>
                    <h1>Welcome back, <em><asp:Label ID="lblDisplayName" runat="server" /></em></h1>
                    <p>Keep your learning streak alive. Your enrolled courses are connected to your CodeQuest account.</p>
                </div>
                <a class="learner-badge" href="Features/Billing/Plans.aspx"><span>Current plan</span><strong><asp:Label ID="lblPlan" runat="server" Text="Basic" /></strong><small>Manage plan &rarr;</small></a>
            </section>

            <asp:Panel ID="pnlMessage" runat="server" CssClass="learner-message" Visible="false" role="alert">
                <asp:Label ID="lblMessage" runat="server" />
            </asp:Panel>

            <section class="learner-stats" aria-label="Learning statistics">
                <article><span>Courses enrolled</span><strong><asp:Label ID="lblCourseCount" runat="server" Text="0" /></strong></article>
                <article><span>Lessons completed</span><strong><asp:Label ID="lblCompletedLessons" runat="server" Text="0" /></strong></article>
                <article><span>Learning streak</span><strong>0 days</strong></article>
                <article><span>Quiz average</span><strong><asp:Label ID="lblQuizAverage" runat="server" Text="--" /></strong></article>
            </section>

            <section id="myLearning" class="learning-section" aria-labelledby="learningTitle">
                <div class="section-heading-row">
                    <div>
                        <p class="section-kicker">My learning</p>
                        <h2 id="learningTitle">Continue your courses.</h2>
                    </div>
                    <a href="Features/Learner/Courses.aspx">Browse courses &rarr;</a>
                </div>

                <asp:Panel ID="pnlEmpty" runat="server" CssClass="learner-message" Visible="false">
                    You have no enrollments yet. Browse the course catalogue to find your first learning path.
                </asp:Panel>

                <div class="enrollment-grid">
                    <asp:Repeater ID="rptEnrollments" runat="server">
                        <ItemTemplate>
                            <article class="enrollment-card">
                                <div class="enrollment-icon">&lt;/&gt;</div>
                                <span class="<%# GetEnrollmentStatusCss(Eval("Status")) %>"><%# Eval("Status") %></span>
                                <p>COURSE-<%# Eval("CourseID") %></p>
                                <h3><%# Server.HtmlEncode(Eval("CourseTitle").ToString()) %></h3>
                                <span class="enrollment-level"><%# Eval("Difficulty") %></span>
                                <a href="Features/Learner/Course.aspx?courseId=<%# Eval("CourseID") %>"><%# GetEnrollmentAction(Eval("Status")) %></a>
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
    <script src="Content/codequest-responsive-nav.js?v=50"></script>
</body>
</html>
