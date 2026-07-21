<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Chapter.aspx.cs" Inherits="CodeQuest.Features.Learner.Chapter" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Chapter | CodeQuest</title>
    <link href="../../Content/codequest-home.css" rel="stylesheet" />
    <link href="../../Content/codequest-chapter.css" rel="stylesheet" />
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
                <a class="active" href="../../LearnerDashboard.aspx#myLearning">My learning</a>
                <a href="../AI/Assistant.aspx">AI assistant</a>
                <a href="Profile.aspx">Profile</a>
                <a href="../../Guest.aspx#about">About</a>
            </nav>
            <div class="header-actions">
                <a class="login-link" href="../../LearnerDashboard.aspx">Dashboard</a>
                <a class="header-cta" href="../../Login.aspx?logout=1">Sign out</a>
            </div>
        </header>

        <main class="chapter-page">
            <asp:Panel ID="pnlError" runat="server" CssClass="chapter-message error" Visible="false" role="alert">
                <asp:Label ID="lblError" runat="server" />
            </asp:Panel>

            <asp:Panel ID="pnlChapter" runat="server" Visible="false">
                <nav class="breadcrumb" aria-label="Breadcrumb">
                    <a href="../../LearnerDashboard.aspx">Dashboard</a>
                    <span>/</span>
                    <a href="../../LearnerDashboard.aspx#myLearning">My learning</a>
                    <span>/</span>
                    <asp:Label ID="lblBreadcrumbCourse" runat="server" />
                    <span>/</span>
                    <asp:Label ID="lblBreadcrumbModule" runat="server" />
                </nav>

                <section class="chapter-heading">
                    <p class="eyebrow"><span></span> Chapter lesson</p>
                    <p class="chapter-code">CHAPTER-<asp:Label ID="lblChapterID" runat="server" /></p>
                    <h1><asp:Label ID="lblTitle" runat="server" /></h1>
                    <p class="chapter-description"><asp:Label ID="lblDescription" runat="server" /></p>
                    <asp:HyperLink ID="lnkCourse" runat="server" CssClass="back-link" Text="&larr; Back to course" />
                    <asp:HyperLink ID="lnkAssistant" runat="server" CssClass="back-link" Text="Ask AI about this chapter &rarr;" />
                </section>

                <asp:Panel ID="pnlTutorial" runat="server" CssClass="lesson-card" Visible="false">
                    <p class="section-kicker">Tutorial</p>
                    <h2><asp:Label ID="lblTutorialTitle" runat="server" /></h2>
                    <pre class="lesson-materials"><asp:Literal ID="litMaterials" runat="server" /></pre>
                </asp:Panel>

                <asp:Panel ID="pnlNoTutorial" runat="server" CssClass="chapter-message" Visible="false">
                    This chapter is published, but its tutorial material has not been added yet.
                </asp:Panel>

                <asp:Panel ID="pnlExercise" runat="server" CssClass="exercise-card" Visible="false">
                    <p class="section-kicker">Practice</p>
                    <h2>Check your understanding.</h2>
                    <p class="exercise-question"><asp:Label ID="lblExerciseQuestion" runat="server" /></p>
                    <div class="answer-row">
                        <asp:TextBox ID="txtAnswer" runat="server" CssClass="answer-input" autocomplete="off" />
                        <asp:Button ID="btnCheckAnswer" runat="server" CssClass="primary-button" Text="Check answer" OnClick="btnCheckAnswer_Click" />
                    </div>
                    <asp:Label ID="lblExerciseResult" runat="server" CssClass="exercise-result" Visible="false" />
                </asp:Panel>

                <asp:Panel ID="pnlQuizLink" runat="server" CssClass="quiz-link-panel" Visible="false">
                    <div>
                        <p class="section-kicker">Checkpoint</p>
                        <p>Ready to test your understanding and save your progress?</p>
                    </div>
                    <asp:HyperLink ID="lnkQuiz" runat="server" CssClass="quiz-link" Text="Take chapter quiz &rarr;" />
                </asp:Panel>
            </asp:Panel>
        </main>

        <footer class="site-footer">
            <span>&copy; 2026 CodeQuest</span>
            <span>Learn &middot; Practise &middot; Build</span>
        </footer>
    </form>
</body>
</html>
