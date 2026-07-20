<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Lessons.aspx.cs" Inherits="CodeQuest.Features.Admin.Lessons" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Lesson Library | CodeQuest</title>
    <link href="../../Content/codequest-home.css" rel="stylesheet" />
    <link href="../../Content/codequest-admin.css" rel="stylesheet" />
    <link href="../../Content/codequest-admin-content.css" rel="stylesheet" />
    <link href="../../Content/codequest-admin-lessons.css" rel="stylesheet" />
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
                <a class="active" href="Lessons.aspx">Lesson library</a>
                <a href="Support.aspx">Support tickets</a>
                <a href="../Public/Courses.aspx">Public courses</a>
            </nav>
            <div class="header-actions">
                <a class="login-link" href="../../Guest.aspx">View site</a>
                <a class="header-cta" href="../../Login.aspx?logout=1">Sign out</a>
            </div>
        </header>

        <main class="lesson-library-page">
            <section class="content-studio-heading">
                <div>
                    <p class="eyebrow"><span></span> Lesson library</p>
                    <h1>Teach every layer.</h1>
                    <p>Public tutorials help guests learn and practise. Chapter quizzes check the work of enrolled learners.</p>
                </div>
                <a class="back-link" href="../../AdminDashboard.aspx">&larr; Back to overview</a>
            </section>

            <asp:Panel ID="pnlError" runat="server" CssClass="studio-message error" Visible="false" role="alert">
                <asp:Label ID="lblError" runat="server" />
            </asp:Panel>
            <asp:Panel ID="pnlSuccess" runat="server" CssClass="studio-message success" Visible="false" role="status">
                <asp:Label ID="lblSuccess" runat="server" />
            </asp:Panel>

            <div class="lesson-grid">
                <section class="studio-card lesson-card">
                    <div class="studio-card-heading">
                        <div><p class="section-kicker">Public learning</p><h2>Tutorials and exercises.</h2></div>
                    </div>
                    <label class="field-label" for="ddlTutorials">Selected tutorial</label>
                    <asp:DropDownList ID="ddlTutorials" runat="server" CssClass="studio-select" AutoPostBack="true" OnSelectedIndexChanged="ddlTutorials_SelectedIndexChanged" />
                    <asp:Label ID="lblSelectedTutorial" runat="server" CssClass="selection-note" />
                    <div class="status-actions">
                        <asp:LinkButton ID="btnPublishTutorial" runat="server" CssClass="mini-action" Text="Publish tutorial" OnClick="btnPublishTutorial_Click" />
                        <asp:LinkButton ID="btnReviewTutorial" runat="server" CssClass="mini-action muted" Text="Move to review" OnClick="btnReviewTutorial_Click" />
                    </div>

                    <div class="studio-form-divider"></div>
                    <p class="form-kicker">New tutorial</p>
                    <label class="field-label" for="txtTutorialTitle">Title</label>
                    <asp:TextBox ID="txtTutorialTitle" runat="server" CssClass="studio-input" MaxLength="200" placeholder="e.g. HTML Links and Images" />
                    <label class="field-label" for="ddlTutorialCategory">Category</label>
                    <asp:DropDownList ID="ddlTutorialCategory" runat="server" CssClass="studio-select">
                        <asp:ListItem Text="HTML" Value="HTML" />
                        <asp:ListItem Text="CSS" Value="CSS" />
                        <asp:ListItem Text="JavaScript" Value="JavaScript" />
                    </asp:DropDownList>
                    <label class="field-label" for="txtTutorialMaterials">Lesson material</label>
                    <asp:TextBox ID="txtTutorialMaterials" runat="server" CssClass="studio-input studio-textarea tall" TextMode="MultiLine" Rows="8" placeholder="Explain the concept and include a code example. Use \n for line breaks in seeded text." />
                    <label class="field-label" for="ddlTutorialStatus">Status</label>
                    <asp:DropDownList ID="ddlTutorialStatus" runat="server" CssClass="studio-select">
                        <asp:ListItem Text="Draft" Value="Draft" />
                        <asp:ListItem Text="Published" Value="Published" />
                        <asp:ListItem Text="Review" Value="Review" />
                    </asp:DropDownList>
                    <asp:Button ID="btnCreateTutorial" runat="server" CssClass="studio-button" Text="Create tutorial" OnClick="btnCreateTutorial_Click" />

                    <div class="studio-list">
                        <p class="form-kicker">Exercises for selected tutorial</p>
                        <asp:Panel ID="pnlNoTutorial" runat="server" CssClass="studio-empty" Visible="false">Select a tutorial to add a guest exercise.</asp:Panel>
                        <asp:Panel ID="pnlExerciseEditor" runat="server" Visible="false">
                            <asp:Repeater ID="rptExercises" runat="server">
                                <ItemTemplate>
                                    <article class="studio-list-item chapter-item"><div><strong><%# Server.HtmlEncode(Eval("Question").ToString()) %></strong><span>Answer: <%# Server.HtmlEncode(Eval("CorrectAnswer").ToString()) %></span></div></article>
                                </ItemTemplate>
                            </asp:Repeater>
                            <label class="field-label" for="txtExerciseQuestion">Question</label>
                            <asp:TextBox ID="txtExerciseQuestion" runat="server" CssClass="studio-input studio-textarea" TextMode="MultiLine" Rows="3" placeholder="What should a guest answer?" />
                            <label class="field-label" for="txtExerciseAnswer">Correct answer</label>
                            <asp:TextBox ID="txtExerciseAnswer" runat="server" CssClass="studio-input" MaxLength="2000" placeholder="e.g. href" />
                            <asp:Button ID="btnCreateExercise" runat="server" CssClass="studio-button secondary-studio-button" Text="Add exercise" OnClick="btnCreateExercise_Click" />
                        </asp:Panel>
                    </div>
                </section>

                <section class="studio-card lesson-card">
                    <div class="studio-card-heading">
                        <div><p class="section-kicker">Learner checkpoint</p><h2>Chapter quizzes.</h2></div>
                    </div>
                    <label class="field-label" for="ddlChapters">Selected chapter</label>
                    <asp:DropDownList ID="ddlChapters" runat="server" CssClass="studio-select" AutoPostBack="true" OnSelectedIndexChanged="ddlChapters_SelectedIndexChanged" />
                    <asp:Label ID="lblSelectedChapter" runat="server" CssClass="selection-note" />

                    <div class="studio-form-divider"></div>
                    <p class="form-kicker">New quiz</p>
                    <label class="field-label" for="txtQuizDescription">Description</label>
                    <asp:TextBox ID="txtQuizDescription" runat="server" CssClass="studio-input" MaxLength="200" placeholder="e.g. HTML foundations checkpoint" />
                    <label class="field-label" for="txtQuizQuestion">Question</label>
                    <asp:TextBox ID="txtQuizQuestion" runat="server" CssClass="studio-input studio-textarea" TextMode="MultiLine" Rows="3" placeholder="What should the learner identify?" />
                    <label class="field-label" for="txtQuizCorrectAnswer">Correct answer</label>
                    <asp:TextBox ID="txtQuizCorrectAnswer" runat="server" CssClass="studio-input" MaxLength="2000" placeholder="Must match one answer choice" />
                    <label class="field-label" for="txtQuizAnswers">Answer choices</label>
                    <asp:TextBox ID="txtQuizAnswers" runat="server" CssClass="studio-input studio-textarea" TextMode="MultiLine" Rows="4" placeholder="One choice per line, or separate choices with commas" />
                    <asp:Button ID="btnCreateQuiz" runat="server" CssClass="studio-button" Text="Create quiz" OnClick="btnCreateQuiz_Click" />

                    <div class="studio-list">
                        <p class="form-kicker">Quizzes for selected chapter</p>
                        <asp:Panel ID="pnlNoChapter" runat="server" CssClass="studio-empty" Visible="false">Select a chapter to add a checkpoint quiz.</asp:Panel>
                        <asp:Repeater ID="rptQuizzes" runat="server">
                            <ItemTemplate>
                                <article class="studio-list-item chapter-item"><div><strong><%# Server.HtmlEncode(Eval("Question").ToString()) %></strong><span><%# Eval("AnswerCount") %> answer choices &middot; Correct: <%# Server.HtmlEncode(Eval("CorrectAnswer").ToString()) %></span></div></article>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </section>
            </div>
        </main>

        <footer class="site-footer">
            <span>&copy; 2026 CodeQuest</span>
            <span>Admin &middot; Teach &middot; Assess</span>
        </footer>
    </form>
</body>
</html>
