<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Tutorials.aspx.cs" Inherits="CodeQuest.Features.Public.Tutorials" %>
<!-- Page purpose: Shows and filters the free tutorial library for guests and signed-in users. -->

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Free Tutorials | CodeQuest</title>
    <link href="../../Content/codequest-home.css?v=50" rel="stylesheet" />
    <link href="../../Content/codequest-tutorials.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <header class="site-header">
            <a class="brand" href="../../Guest.aspx" aria-label="CodeQuest home">
                <img class="brand-logo" src="../../Content/Images/CodeQuest_logo.png" alt="CodeQuest" />
            </a>
            <nav class="main-nav" aria-label="Main navigation">
                <asp:PlaceHolder ID="phPublicNavigation" runat="server">
                    <a href="../../Guest.aspx">Home</a>
                    <a href="Courses.aspx">Courses</a>
                    <a class="active" href="Tutorials.aspx">Tutorials</a>
                    <a href="../../Guest.aspx#about">About</a>
                </asp:PlaceHolder>
                <asp:PlaceHolder ID="phAdminNavigation" runat="server" Visible="false">
                    <a href="../../AdminDashboard.aspx">Overview</a>
                    <a href="../Admin/Content.aspx">Content studio</a>
                    <a href="../Admin/Lessons.aspx">Lesson library</a>
                    <a href="../Admin/Users.aspx">Users</a>
                    <a href="../Admin/Support.aspx">Support tickets</a>
                    <a href="Courses.aspx">Preview courses</a>
                    <a class="active" href="Tutorials.aspx">Preview tutorials</a>
                </asp:PlaceHolder>
            </nav>
            <div class="header-actions">
                <asp:PlaceHolder ID="phPublicActions" runat="server">
                    <a class="login-link" href="../../Login.aspx">Login</a>
                    <a class="header-cta" href="../../Register.aspx">Get Started</a>
                </asp:PlaceHolder>
                <asp:PlaceHolder ID="phLearnerActions" runat="server" Visible="false">
                    <a class="login-link" href="../../LearnerDashboard.aspx">Dashboard</a>
                    <a class="header-cta session-cta" href="../../Login.aspx?logout=1">Sign out</a>
                </asp:PlaceHolder>
                <asp:PlaceHolder ID="phAdminActions" runat="server" Visible="false">
                    <a class="login-link" href="../../Guest.aspx">View site</a>
                    <a class="header-cta" href="../../Login.aspx?logout=1">Sign out</a>
                </asp:PlaceHolder>
            </div>
        </header>

        <main class="tutorials-page">
            <section class="tutorials-hero">
                <p class="eyebrow"><span></span> Free access</p>
                <h1>Learn the basics.<br /><em>Build in public.</em></h1>
                <p>Read beginner-friendly HTML, CSS and JavaScript tutorials and try the exercises without creating an account. Sign in only when you want to save progress or take quizzes.</p>
            </section>

            <asp:Panel ID="pnlAdminPreview" runat="server" CssClass="tutorial-message" Visible="false">
                Admin preview mode: open any draft, review or published tutorial to verify its lesson material and test its exercise.
            </asp:Panel>

            <nav class="tutorial-category-nav" aria-label="Tutorial categories">
                <a class="category-link" href="Tutorials.aspx">All tutorials</a>
                <a class="category-link html" href="Tutorials.aspx?category=HTML">HTML</a>
                <a class="category-link css" href="Tutorials.aspx?category=CSS">CSS</a>
                <a class="category-link javascript" href="Tutorials.aspx?category=JavaScript">JavaScript</a>
            </nav>

            <div class="tutorial-results-heading">
                <p class="section-kicker">Free learning library</p>
                <h2><asp:Label ID="lblCategoryTitle" runat="server" Text="All tutorials" /></h2>
                <p>Choose a category, open a lesson and practise immediately.</p>
            </div>

            <asp:Panel ID="pnlError" runat="server" CssClass="tutorial-message error" Visible="false" role="alert">
                <asp:Label ID="lblError" runat="server" />
            </asp:Panel>

            <asp:Panel ID="pnlEmpty" runat="server" CssClass="tutorial-message" Visible="false">
                No tutorials are available in this category yet.
            </asp:Panel>

            <section class="tutorial-grid" aria-label="Free tutorials">
                <asp:Repeater ID="rptTutorials" runat="server">
                    <ItemTemplate>
                        <article class="tutorial-card">
                            <div class="tutorial-card-topline">
                                <span>FREE TUTORIAL</span>
                                <span class="tutorial-level"><%# Server.HtmlEncode(Eval("Category").ToString()) %></span>
                            </div>
                            <p class="module-label">Tutorial-<%# Eval("TutorialID") %></p>
                            <h2><%# Server.HtmlEncode(Eval("Title").ToString()) %></h2>
                            <p><%# Server.HtmlEncode(Eval("Materials") == null ? "Read the lesson and try the practice question." : Eval("Materials").ToString()) %></p>
                            <a href="Tutorial.aspx?tutorialId=<%# Eval("TutorialID") %>"><%# GetTutorialActionText() %> &rarr;</a>
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
