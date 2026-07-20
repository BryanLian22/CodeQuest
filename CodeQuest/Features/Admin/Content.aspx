<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Content.aspx.cs" Inherits="CodeQuest.Features.Admin.Content" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Content Studio | CodeQuest</title>
    <link href="../../Content/codequest-home.css" rel="stylesheet" />
    <link href="../../Content/codequest-admin.css" rel="stylesheet" />
    <link href="../../Content/codequest-admin-content.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <header class="site-header">
            <a class="brand" href="../../Guest.aspx" aria-label="CodeQuest home">
                <img class="brand-logo" src="../../Content/Images/CodeQuest_logo.png" alt="CodeQuest" />
            </a>
            <nav class="main-nav" aria-label="Admin navigation">
                <a href="../../AdminDashboard.aspx">Overview</a>
                <a class="active" href="Content.aspx">Content studio</a>
                <a href="Lessons.aspx">Lesson library</a>
                <a href="Support.aspx">Support tickets</a>
                <a href="../Public/Courses.aspx">Public courses</a>
                <a href="../Public/Tutorials.aspx">Tutorial library</a>
            </nav>
            <div class="header-actions">
                <a class="login-link" href="../../Guest.aspx">View site</a>
                <a class="header-cta" href="../../Login.aspx?logout=1">Sign out</a>
            </div>
        </header>

        <main class="content-studio-page">
            <section class="content-studio-heading">
                <div>
                    <p class="eyebrow"><span></span> Content studio</p>
                    <h1>Build the learning path.</h1>
                    <p>Create courses, organise them into modules and add chapters for enrolled learners. Draft modules stay hidden until you publish them.</p>
                </div>
                <a class="back-link" href="../../AdminDashboard.aspx">&larr; Back to overview</a>
            </section>

            <asp:Panel ID="pnlError" runat="server" CssClass="studio-message error" Visible="false" role="alert">
                <asp:Label ID="lblError" runat="server" />
            </asp:Panel>
            <asp:Panel ID="pnlSuccess" runat="server" CssClass="studio-message success" Visible="false" role="status">
                <asp:Label ID="lblSuccess" runat="server" />
            </asp:Panel>

            <div class="studio-grid">
                <section class="studio-card">
                    <div class="studio-card-heading">
                        <div><p class="section-kicker">1 &middot; Course</p><h2>Choose or create a course.</h2></div>
                    </div>
                    <label class="field-label" for="ddlCourses">Selected course</label>
                    <asp:DropDownList ID="ddlCourses" runat="server" CssClass="studio-select" AutoPostBack="true" OnSelectedIndexChanged="ddlCourses_SelectedIndexChanged" />
                    <asp:Label ID="lblSelectedCourse" runat="server" CssClass="selection-note" />

                    <div class="studio-form-divider"></div>
                    <p class="form-kicker">New course</p>
                    <label class="field-label" for="txtCourseTitle">Title</label>
                    <asp:TextBox ID="txtCourseTitle" runat="server" CssClass="studio-input" MaxLength="150" placeholder="e.g. Web Accessibility Foundations" />
                    <label class="field-label" for="txtCourseDescription">Description</label>
                    <asp:TextBox ID="txtCourseDescription" runat="server" CssClass="studio-input studio-textarea" TextMode="MultiLine" Rows="4" placeholder="What will learners build or understand?" />
                    <label class="field-label" for="ddlCourseDifficulty">Difficulty</label>
                    <asp:DropDownList ID="ddlCourseDifficulty" runat="server" CssClass="studio-select">
                        <asp:ListItem Text="Beginner" Value="Beginner" />
                        <asp:ListItem Text="Intermediate" Value="Intermediate" />
                        <asp:ListItem Text="Advanced" Value="Advanced" />
                    </asp:DropDownList>
                    <asp:Button ID="btnCreateCourse" runat="server" CssClass="studio-button" Text="Create course" OnClick="btnCreateCourse_Click" />
                </section>

                <section class="studio-card">
                    <div class="studio-card-heading">
                        <div><p class="section-kicker">2 &middot; Module</p><h2>Organise the course.</h2></div>
                    </div>
                    <asp:Panel ID="pnlNoCourse" runat="server" CssClass="studio-empty" Visible="false">Create a course first, then add its modules.</asp:Panel>
                    <asp:Panel ID="pnlModuleEditor" runat="server" Visible="false">
                        <label class="field-label" for="ddlModules">Selected module</label>
                        <asp:DropDownList ID="ddlModules" runat="server" CssClass="studio-select" AutoPostBack="true" OnSelectedIndexChanged="ddlModules_SelectedIndexChanged" />
                        <asp:Label ID="lblSelectedModule" runat="server" CssClass="selection-note" />

                        <div class="studio-form-divider"></div>
                        <p class="form-kicker">New module</p>
                        <label class="field-label" for="txtModuleTitle">Title</label>
                        <asp:TextBox ID="txtModuleTitle" runat="server" CssClass="studio-input" MaxLength="150" placeholder="e.g. Semantic HTML" />
                        <label class="field-label" for="txtModuleDescription">Description</label>
                        <asp:TextBox ID="txtModuleDescription" runat="server" CssClass="studio-input studio-textarea" TextMode="MultiLine" Rows="3" placeholder="What does this module cover?" />
                        <label class="field-label" for="ddlModuleStatus">Initial status</label>
                        <asp:DropDownList ID="ddlModuleStatus" runat="server" CssClass="studio-select">
                            <asp:ListItem Text="Draft" Value="Draft" />
                            <asp:ListItem Text="Published" Value="Published" />
                        </asp:DropDownList>
                        <asp:Button ID="btnCreateModule" runat="server" CssClass="studio-button" Text="Add module" OnClick="btnCreateModule_Click" />

                        <div class="studio-list">
                            <p class="form-kicker">Existing modules</p>
                            <asp:Repeater ID="rptModules" runat="server">
                                <ItemTemplate>
                                    <article class="studio-list-item">
                                        <div><strong><%# Server.HtmlEncode(Eval("Title").ToString()) %></strong><span><%# Eval("ChapterCount") %> chapters &middot; <%# Eval("Status") %></span></div>
                                        <asp:LinkButton ID="btnPublishModule" runat="server" CssClass="mini-action" CommandName="Publish" CommandArgument='<%# Eval("ModuleID") %>' OnCommand="btnModuleStatus_Command">Publish</asp:LinkButton>
                                        <asp:LinkButton ID="btnArchiveModule" runat="server" CssClass="mini-action muted" CommandName="Archive" CommandArgument='<%# Eval("ModuleID") %>' OnCommand="btnModuleStatus_Command">Archive</asp:LinkButton>
                                    </article>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </asp:Panel>
                </section>

                <section class="studio-card">
                    <div class="studio-card-heading">
                        <div><p class="section-kicker">3 &middot; Chapter</p><h2>Add the lessons.</h2></div>
                    </div>
                    <asp:Panel ID="pnlNoModule" runat="server" CssClass="studio-empty" Visible="false">Choose a module to add learner chapters.</asp:Panel>
                    <asp:Panel ID="pnlChapterEditor" runat="server" Visible="false">
                        <label class="field-label" for="txtChapterTitle">Title</label>
                        <asp:TextBox ID="txtChapterTitle" runat="server" CssClass="studio-input" MaxLength="150" placeholder="e.g. Headings and paragraphs" />
                        <label class="field-label" for="txtChapterDescription">Description</label>
                        <asp:TextBox ID="txtChapterDescription" runat="server" CssClass="studio-input studio-textarea" TextMode="MultiLine" Rows="3" placeholder="What will the learner practise?" />
                        <asp:Button ID="btnCreateChapter" runat="server" CssClass="studio-button" Text="Add chapter" OnClick="btnCreateChapter_Click" />

                        <div class="studio-list">
                            <p class="form-kicker">Existing chapters</p>
                            <asp:Repeater ID="rptChapters" runat="server">
                                <ItemTemplate>
                                    <article class="studio-list-item chapter-item">
                                        <div><strong><%# Server.HtmlEncode(Eval("Title").ToString()) %></strong><span>CHAPTER-<%# Eval("ChapterID") %></span></div>
                                    </article>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </asp:Panel>
                </section>
            </div>
        </main>

        <footer class="site-footer">
            <span>&copy; 2026 CodeQuest</span>
            <span>Admin &middot; Create &middot; Publish</span>
        </footer>
    </form>
</body>
</html>
