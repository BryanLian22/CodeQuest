<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Quiz.aspx.cs" Inherits="CodeQuest.Features.Learner.Quiz" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Chapter quiz | CodeQuest</title>
    <link href="../../Content/codequest-home.css" rel="stylesheet" />
    <link href="../../Content/codequest-quiz.css?v=38" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <header class="site-header">
            <a class="brand" href="../../Guest.aspx" aria-label="CodeQuest home">
                <img class="brand-logo" src="../../Content/Images/CodeQuest_logo.png" alt="CodeQuest" />
            </a>
            <nav class="main-nav" aria-label="Learner navigation">
                <asp:PlaceHolder ID="phLearnerNavigation" runat="server">
                    <a href="../../LearnerDashboard.aspx">Dashboard</a>
                    <a href="Courses.aspx">Courses</a>
                    <a class="active" href="../../LearnerDashboard.aspx#myLearning">My learning</a>
                    <a href="../AI/Assistant.aspx">AI assistant</a>
                    <a href="Profile.aspx">Profile</a>
                    <a href="../Support/Tickets.aspx">Support</a>
                </asp:PlaceHolder>
                <asp:PlaceHolder ID="phAdminNavigation" runat="server" Visible="false">
                    <a href="../../AdminDashboard.aspx">Overview</a>
                    <a href="../Admin/Content.aspx">Content studio</a>
                    <a href="../Admin/Lessons.aspx">Lesson library</a>
                    <a href="../Admin/Users.aspx">Users</a>
                    <a href="../Admin/Support.aspx">Support tickets</a>
                    <a class="active" href="../Public/Courses.aspx">Preview courses</a>
                    <a href="../Public/Tutorials.aspx">Preview tutorials</a>
                </asp:PlaceHolder>
            </nav>
            <div class="header-actions">
                <asp:PlaceHolder ID="phLearnerActions" runat="server">
                    <a class="login-link" href="../../LearnerDashboard.aspx">Dashboard</a>
                    <a class="header-cta" href="../../Login.aspx?logout=1">Sign out</a>
                </asp:PlaceHolder>
                <asp:PlaceHolder ID="phAdminActions" runat="server" Visible="false">
                    <a class="login-link" href="../Public/Courses.aspx">All previews</a>
                    <a class="header-cta" href="../../Login.aspx?logout=1">Sign out</a>
                </asp:PlaceHolder>
            </div>
        </header>

        <main class="quiz-page">
            <asp:Panel ID="pnlError" runat="server" CssClass="quiz-message error" Visible="false" role="alert">
                <asp:Label ID="lblError" runat="server" />
            </asp:Panel>

            <asp:Panel ID="pnlAdminPreview" runat="server" CssClass="quiz-message" Visible="false">
                Admin preview mode: submit this quiz to test its scoring. The attempt will not be saved to learner progress.
            </asp:Panel>

            <asp:Panel ID="pnlQuiz" runat="server" Visible="false">
                <nav class="breadcrumb" aria-label="Breadcrumb">
                    <asp:PlaceHolder ID="phLearnerBreadcrumb" runat="server">
                        <a href="../../LearnerDashboard.aspx">Dashboard</a>
                        <span>/</span>
                        <a href="../../LearnerDashboard.aspx#myLearning">My learning</a>
                        <span>/</span>
                    </asp:PlaceHolder>
                    <asp:PlaceHolder ID="phAdminBreadcrumb" runat="server" Visible="false">
                        <a href="../../AdminDashboard.aspx">Admin</a>
                        <span>/</span>
                        <a href="../Public/Courses.aspx">Course previews</a>
                        <span>/</span>
                    </asp:PlaceHolder>
                    <asp:HyperLink ID="lnkBreadcrumbCourse" runat="server" />
                    <span>/</span>
                    <asp:HyperLink ID="lnkBreadcrumbChapter" runat="server" />
                </nav>

                <section class="quiz-heading">
                    <p class="eyebrow"><span></span> Chapter quiz</p>
                    <p class="quiz-code">CHAPTER-<asp:Label ID="lblChapterID" runat="server" /></p>
                    <h1>Check your understanding.</h1>
                    <p class="quiz-intro">Answer the questions from <strong><asp:Label ID="lblChapterTitle" runat="server" /></strong>. A score of 75% or higher is required to pass.</p>
                    <asp:HyperLink ID="lnkBackToChapter" runat="server" CssClass="back-link" Text="&larr; Back to chapter" />
                </section>

                <asp:Panel ID="pnlNoQuiz" runat="server" CssClass="quiz-message" Visible="false">
                    This chapter does not have a quiz yet. You can return to the chapter and continue learning.
                </asp:Panel>

                <asp:Panel ID="pnlQuestions" runat="server" Visible="false" CssClass="question-list">
                    <asp:Repeater ID="rptQuizzes" runat="server">
                        <ItemTemplate>
                            <article class="question-card">
                                <asp:HiddenField ID="hidQuizID" runat="server" Value='<%# Eval("QuizID") %>' />
                                <p class="question-number">QUESTION <%# Container.ItemIndex + 1 %></p>
                                <h2><%# Server.HtmlEncode(Eval("Question").ToString()) %></h2>
                                <asp:RadioButtonList ID="rblAnswers" runat="server" CssClass="answer-list"
                                    DataSource='<%# Eval("Answers") %>' DataTextField="Answer" DataValueField="Answer" />
                            </article>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:Button ID="btnSubmitQuiz" runat="server" CssClass="primary-button" Text="Submit quiz" OnClick="btnSubmitQuiz_Click" />
                    <asp:Panel ID="pnlResult" runat="server" CssClass="quiz-result" Visible="false" role="status">
                        <asp:Label ID="lblResult" runat="server" />
                        <asp:Label ID="lblSaveNotice" runat="server" CssClass="save-notice" Visible="false" />
                        <div class="quiz-result-actions">
                            <asp:HyperLink ID="lnkRetakeQuiz" runat="server" CssClass="retake-quiz-link" Visible="false" Text="Retake quiz" />
                            <asp:HyperLink ID="lnkNextChapter" runat="server" CssClass="next-chapter-link" Visible="false" />
                        </div>
                    </asp:Panel>
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
