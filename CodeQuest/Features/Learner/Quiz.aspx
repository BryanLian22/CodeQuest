<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Quiz.aspx.cs" Inherits="CodeQuest.Features.Learner.Quiz" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Chapter quiz | CodeQuest</title>
    <link href="../../Content/codequest-home.css" rel="stylesheet" />
    <link href="../../Content/codequest-quiz.css" rel="stylesheet" />
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
                <a href="../../Guest.aspx#about">About</a>
            </nav>
            <div class="header-actions">
                <a class="login-link" href="../../LearnerDashboard.aspx">Dashboard</a>
                <a class="header-cta" href="../../LearnerDashboard.aspx#myLearning">My learning</a>
            </div>
        </header>

        <main class="quiz-page">
            <asp:Panel ID="pnlError" runat="server" CssClass="quiz-message error" Visible="false" role="alert">
                <asp:Label ID="lblError" runat="server" />
            </asp:Panel>

            <asp:Panel ID="pnlQuiz" runat="server" Visible="false">
                <nav class="breadcrumb" aria-label="Breadcrumb">
                    <a href="../../LearnerDashboard.aspx">Dashboard</a>
                    <span>/</span>
                    <a href="../../LearnerDashboard.aspx#myLearning">My learning</a>
                    <span>/</span>
                    <asp:Label ID="lblBreadcrumbCourse" runat="server" />
                    <span>/</span>
                    <asp:Label ID="lblBreadcrumbChapter" runat="server" />
                </nav>

                <section class="quiz-heading">
                    <p class="eyebrow"><span></span> Chapter quiz</p>
                    <p class="quiz-code">CHAPTER-<asp:Label ID="lblChapterID" runat="server" /></p>
                    <h1>Check your understanding.</h1>
                    <p class="quiz-intro">Answer the questions from <strong><asp:Label ID="lblChapterTitle" runat="server" /></strong> to save your progress.</p>
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
