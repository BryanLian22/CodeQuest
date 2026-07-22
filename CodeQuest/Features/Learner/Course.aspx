<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Course.aspx.cs" Inherits="CodeQuest.Features.Learner.Course" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Course | CodeQuest</title>
    <link href="../../Content/codequest-home.css" rel="stylesheet" />
    <link href="../../Content/codequest-course.css?v=37" rel="stylesheet" />
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
                    <a class="active" href="Courses.aspx">Courses</a>
                    <a href="../../LearnerDashboard.aspx#myLearning">My learning</a>
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

        <main class="course-page">
            <asp:HyperLink ID="lnkBack" runat="server" CssClass="back-link" NavigateUrl="../../LearnerDashboard.aspx" Text="&larr; Back to my learning" />

            <asp:Panel ID="pnlError" runat="server" CssClass="course-message error" Visible="false" role="alert">
                <asp:Label ID="lblError" runat="server" />
            </asp:Panel>

            <asp:Panel ID="pnlAdminPreview" runat="server" CssClass="course-message" Visible="false">
                Admin preview mode: open any chapter from draft, published or archived modules to test its lesson, exercise and quiz. Preview activity does not change learner progress.
            </asp:Panel>

            <asp:Panel ID="pnlCourse" runat="server" Visible="false">
                <section class="course-heading">
                    <p class="eyebrow"><span></span> Course workspace</p>
                    <span class="course-code">COURSE-<asp:Label ID="lblCourseID" runat="server" /></span>
                    <h1><asp:Label ID="lblTitle" runat="server" /></h1>
                    <p class="course-description"><asp:Label ID="lblDescription" runat="server" /></p>
                    <span class="course-level"><asp:Label ID="lblDifficulty" runat="server" /></span>
                </section>

                <asp:Panel ID="pnlNotEnrolled" runat="server" CssClass="course-message" Visible="false">
                    You have not enrolled in this course yet.
                    <asp:HyperLink ID="lnkEnroll" runat="server" CssClass="inline-link" Text="Enrol now &rarr;" />
                </asp:Panel>

                <asp:Panel ID="pnlContent" runat="server" Visible="false">
                    <div class="content-heading">
                        <div>
                            <p class="section-kicker">Course content</p>
                            <h2>Learn step by step.</h2>
                        </div>
                        <span class="content-note">Published modules</span>
                    </div>

                    <asp:Panel ID="pnlNoContent" runat="server" CssClass="course-message" Visible="false">
                        Your course is enrolled, but the administrator has not published any modules yet.
                    </asp:Panel>

                    <div class="module-grid">
                        <asp:Repeater ID="rptModules" runat="server">
                            <ItemTemplate>
                                <article id="module-<%# Eval("ModuleID") %>" class="module-card">
                                    <div class="module-topline">
                                        <span>MODULE-<%# Eval("ModuleID") %></span>
                                        <span class="module-status"><%# Eval("Status") %></span>
                                    </div>
                                    <h3><%# Server.HtmlEncode(Eval("Title").ToString()) %></h3>
                                    <p><%# Server.HtmlEncode(Eval("Description") == null ? "Work through the chapters in this learning module." : Eval("Description").ToString()) %></p>
                                    <div class="chapter-list">
                                        <asp:Repeater ID="rptChapters" runat="server" DataSource='<%# Eval("Chapters") %>'>
                                            <ItemTemplate>
                                                <div class="chapter-row">
                                                    <span class="chapter-number">CHAPTER</span>
                                                    <div>
                                                        <div class="chapter-title-row">
                                                            <a class="chapter-link" href="Chapter.aspx?chapterId=<%# Eval("ChapterID") %>"><%# Server.HtmlEncode(Eval("Title").ToString()) %></a>
                                                            <span class="chapter-complete"><%# System.Convert.ToBoolean(Eval("IsCompleted")) ? "DONE" : "" %></span>
                                                        </div>
                                                        <span><%# Server.HtmlEncode(Eval("Description") == null ? "Guided lesson" : Eval("Description").ToString()) %></span>
                                                    </div>
                                                </div>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </div>
                                </article>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
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
