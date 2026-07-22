<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Enroll.aspx.cs" Inherits="CodeQuest.Features.Learner.Enroll" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Enrol in a course | CodeQuest</title>
    <link href="../../Content/codequest-home.css" rel="stylesheet" />
    <link href="../../Content/codequest-enroll.css" rel="stylesheet" />
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

        <main class="enrol-page">
            <a class="back-link" href="Courses.aspx">&larr; Back to courses</a>

            <asp:Panel ID="pnlError" runat="server" CssClass="enrol-message error" Visible="false" role="alert">
                <asp:Label ID="lblError" runat="server" />
            </asp:Panel>

            <asp:Panel ID="pnlCourse" runat="server" CssClass="enrol-layout" Visible="false">
                <section class="course-summary">
                    <p class="eyebrow"><span></span> Course enrolment</p>
                    <span class="course-code">COURSE-<asp:Label ID="lblCourseID" runat="server" /></span>
                    <h1><asp:Label ID="lblTitle" runat="server" /></h1>
                    <p class="course-description"><asp:Label ID="lblDescription" runat="server" /></p>
                    <div class="course-meta">
                        <span>Difficulty</span>
                        <strong><asp:Label ID="lblDifficulty" runat="server" /></strong>
                    </div>
                    <p class="course-note">Your enrolment is saved to your CodeQuest account and will appear in My learning.</p>
                </section>

                <aside class="enrol-card">
                    <p class="card-kicker">Your access</p>
                    <h2><asp:Label ID="lblPlan" runat="server" Text="Basic" /> plan</h2>
                    <asp:Panel ID="pnlPlanAllowed" runat="server">
                        <p class="card-copy">This course is available with your current plan.</p>
                        <asp:Button ID="btnEnroll" runat="server" CssClass="primary-button enrol-button" Text="Enrol in course" OnClick="btnEnroll_Click" />
                    </asp:Panel>
                    <asp:Panel ID="pnlLocked" runat="server" Visible="false" CssClass="locked-panel">
                        <p class="card-copy"><asp:Label ID="lblLocked" runat="server" /></p>
                        <a class="primary-button" href="../Billing/Plans.aspx">Upgrade to Premium</a>
                        <a class="secondary-button" href="../../Guest.aspx#about">Back to home</a>
                    </asp:Panel>
                    <asp:Panel ID="pnlAlreadyEnrolled" runat="server" Visible="false" CssClass="already-panel">
                        <p class="card-copy">You are already enrolled in this course.</p>
                        <a class="primary-button" href="../../LearnerDashboard.aspx">Go to My learning</a>
                    </asp:Panel>
                </aside>
            </asp:Panel>
        </main>

        <footer class="site-footer">
            <span>&copy; 2026 CodeQuest</span>
            <span>Learn &middot; Practise &middot; Build</span>
        </footer>
    </form>
</body>
</html>
