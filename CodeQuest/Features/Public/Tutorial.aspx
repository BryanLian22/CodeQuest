<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Tutorial.aspx.cs" Inherits="CodeQuest.Features.Public.Tutorial" %>
<!-- Page purpose: Shows one free public tutorial and its exercise without requiring enrolment. -->

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Tutorial | CodeQuest</title>
    <link href="../../Content/codequest-home.css?v=50" rel="stylesheet" />
    <link href="../../Content/codequest-tutorial.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <header class="site-header">
            <a class="brand" href="../../Guest.aspx" aria-label="CodeQuest home">
                <img class="brand-logo" src="../../Content/Images/CodeQuest_logo.png" alt="CodeQuest" />
            </a>
            <nav class="main-nav" aria-label="Public navigation">
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

        <main class="tutorial-page">
            <a class="tutorial-back" href="Tutorials.aspx">&larr; Back to tutorial library</a>

            <asp:Panel ID="pnlError" runat="server" CssClass="tutorial-message error" Visible="false" role="alert">
                <asp:Label ID="lblError" runat="server" />
            </asp:Panel>

            <asp:Panel ID="pnlAdminPreview" runat="server" CssClass="tutorial-message" Visible="false">
                Admin preview mode: use the exercise below to verify that this tutorial behaves correctly before or after publication.
            </asp:Panel>

            <asp:Panel ID="pnlTutorial" runat="server" Visible="false">
                <section class="tutorial-heading">
                    <p class="eyebrow"><span></span> Public tutorial</p>
                    <p class="tutorial-code">TUTORIAL-<asp:Label ID="lblTutorialID" runat="server" /></p>
                    <p class="tutorial-category"><asp:Label ID="lblCategory" runat="server" /></p>
                    <h1><asp:Label ID="lblTitle" runat="server" /></h1>
                    <p>Free access for everyone. Read the explanation and try the exercise before deciding whether to create an account.</p>
                </section>

                <asp:Panel ID="pnlHtmlGuide" runat="server" CssClass="html-guide" Visible="false">
                    <div class="html-guide-copy">
                        <p class="section-kicker">HTML documents</p>
                        <h2>Every page starts with a document structure.</h2>
                        <p>All HTML documents start with a document type declaration:</p>
                        <p><code>&lt;!DOCTYPE html&gt;</code></p>
                        <p>The document itself begins with <code>&lt;html&gt;</code> and ends with <code>&lt;/html&gt;</code>. The visible part of the page belongs between <code>&lt;body&gt;</code> and <code>&lt;/body&gt;</code>.</p>

                        <div class="html-example">
                            <h3>Example</h3>
                            <pre><code>&lt;!DOCTYPE html&gt;
&lt;html&gt;
&lt;head&gt;
  &lt;title&gt;My first page&lt;/title&gt;
&lt;/head&gt;
&lt;body&gt;
  &lt;h1&gt;My First Heading&lt;/h1&gt;
  &lt;p&gt;My first paragraph.&lt;/p&gt;
&lt;/body&gt;
&lt;/html&gt;</code></pre>
                        </div>

                        <h2>The &lt;!DOCTYPE&gt; declaration</h2>
                        <p>The declaration represents the document type and helps browsers display the page correctly. It appears once at the top, before any HTML tags, and is not case sensitive.</p>
                        <pre class="short-code"><code>&lt;!DOCTYPE html&gt;</code></pre>
                    </div>
                    <aside class="html-structure-visual" aria-label="HTML document structure map">
                        <p class="visual-label">CODEQUEST MAP</p>
                        <h3>One document.<br />Two main areas.</h3>
                        <div class="structure-tree">
                            <div class="structure-node root-node">
                                <span>&lt;html&gt;</span>
                                <div class="structure-node"><span>&lt;head&gt;</span><small>metadata and title</small></div>
                                <div class="structure-node body-node"><span>&lt;body&gt;</span><small>visible page content</small></div>
                            </div>
                        </div>
                        <p class="visual-note">The browser reads the document from the outside in: the root contains the head and body.</p>
                    </aside>
                </asp:Panel>

                <asp:Panel ID="pnlMaterials" runat="server" CssClass="tutorial-material-card" Visible="false">
                    <p class="section-kicker">Lesson material</p>
                    <pre class="tutorial-materials"><asp:Literal ID="litMaterials" runat="server" /></pre>
                </asp:Panel>

                <asp:Panel ID="pnlNoMaterials" runat="server" CssClass="tutorial-message" Visible="false">
                    This tutorial has been published, but its lesson material has not been added yet.
                </asp:Panel>

                <asp:Panel ID="pnlExercise" runat="server" CssClass="tutorial-exercise-card" Visible="false">
                    <p class="section-kicker">Exercise</p>
                    <h2>Try it yourself.</h2>
                    <p class="exercise-question"><asp:Label ID="lblExerciseQuestion" runat="server" /></p>
                    <div class="answer-row">
                        <asp:TextBox ID="txtAnswer" runat="server" CssClass="answer-input" autocomplete="off" />
                        <asp:Button ID="btnCheckAnswer" runat="server" CssClass="primary-button" Text="Check answer" OnClick="btnCheckAnswer_Click" />
                    </div>
                    <asp:Label ID="lblExerciseResult" runat="server" CssClass="exercise-result" Visible="false" />
                </asp:Panel>
            </asp:Panel>
        </main>

        <footer class="site-footer">
            <span>&copy; 2026 CodeQuest</span>
            <span>Learn &middot; Practise &middot; Build</span>
        </footer>
    </form>
    <script src="../../Content/codequest-responsive-nav.js?v=50"></script>
</body>
</html>
