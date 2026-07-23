<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Assistant.aspx.cs" Inherits="CodeQuest.Features.AI.Assistant" %>
<!-- Page purpose: Presents the premium learning assistant, current lesson context and chat interaction. -->

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>AI learning assistant | CodeQuest</title>
    <link href="../../Content/codequest-home.css?v=50" rel="stylesheet" />
    <link href="../../Content/codequest-ai.css?v=42" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <header class="site-header">
            <a class="brand" href="../../Guest.aspx" aria-label="CodeQuest home">
                <img class="brand-logo" src="../../Content/Images/CodeQuest_logo.png" alt="CodeQuest" />
            </a>
            <nav class="main-nav" aria-label="Learner navigation">
                <a href="../../LearnerDashboard.aspx">Dashboard</a>
                <a href="../Learner/Courses.aspx">Courses</a>
                <a href="../../LearnerDashboard.aspx#myLearning">My learning</a>
                <a class="active" href="Assistant.aspx">AI assistant</a>
                <a href="../Learner/Profile.aspx">Profile</a>
                <a href="../Support/Tickets.aspx">Support</a>
            </nav>
            <div class="header-actions">
                <a class="login-link" href="../Billing/Plans.aspx">Manage plan</a>
                <a class="header-cta" href="../../Login.aspx?logout=1">Sign out</a>
            </div>
        </header>

        <main class="ai-page">
            <section class="ai-heading">
                <div>
                    <p class="eyebrow"><span></span> Premium feature</p>
                    <h1>Learn with a<br /><em>second pair of eyes.</em></h1>
                    <p>Ask for explanations, examples and hints while staying inside your current lesson.</p>
                </div>
                <div class="ai-badge"><span>AI learning assistant</span><strong>Premium</strong><small>Course-aware guidance</small></div>
            </section>

            <asp:Panel ID="pnlError" runat="server" CssClass="ai-message error" Visible="false" role="alert">
                <asp:Label ID="lblError" runat="server" />
            </asp:Panel>

            <asp:Panel ID="pnlLocked" runat="server" CssClass="locked-card" Visible="false">
                <div class="locked-icon" aria-hidden="true">&#10022;</div>
                <div class="locked-content">
                    <p class="section-kicker">Premium only</p>
                    <h2>Unlock your learning copilot.</h2>
                    <p>Upgrade to Premium to ask CodeQuest AI about HTML, CSS, JavaScript and your enrolled chapters.</p>
                    <a class="ai-button locked-action" href="../Billing/Plans.aspx">View Premium &rarr;</a>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlAssistant" runat="server" Visible="false">
                <div class="ai-layout">
                    <aside class="context-card">
                        <p class="section-kicker">Current context</p>
                        <h2><asp:Label ID="lblContextTitle" runat="server" /></h2>
                        <div class="context-item"><span>Course</span><strong><asp:Label ID="lblCourse" runat="server" /></strong></div>
                        <div class="context-item"><span>Module</span><strong><asp:Label ID="lblModule" runat="server" /></strong></div>
                        <div class="context-item"><span>Chapter</span><strong><asp:Label ID="lblChapter" runat="server" /></strong></div>
                        <p class="context-note">The assistant uses this context to keep explanations relevant. It does not replace your course chapter or quiz.</p>
                    </aside>

                    <section class="chat-card">
                        <div class="chat-heading"><div><p class="section-kicker">CodeQuest AI</p><h2>What are you building?</h2></div><asp:Button ID="btnClear" runat="server" CssClass="clear-button" Text="Clear chat" CausesValidation="false" OnClick="btnClear_Click" /></div>
                        <div class="prompt-list" aria-label="Suggested prompts">
                            <button type="button" class="prompt-chip" onclick="setPrompt('Explain this HTML concept in a simple way.')">Explain a concept</button>
                            <button type="button" class="prompt-chip" onclick="setPrompt('Give me a small example and explain each line.')">Show an example</button>
                            <button type="button" class="prompt-chip" onclick="setPrompt('Give me a hint for debugging my code without giving the final quiz answer.')">Give me a hint</button>
                        </div>
                        <div class="chat-thread">
                            <asp:Repeater ID="rptMessages" runat="server">
                                <ItemTemplate>
                                    <article class="chat-bubble <%# Eval("Role") %>"><span class="bubble-label"><%# Eval("Role").ToString() == "assistant" ? "CodeQuest AI" : "You" %></span><p><%# Server.HtmlEncode(Eval("Content").ToString()).Replace("\n", "<br />") %></p></article>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                        <div class="composer">
                            <label class="sr-only" for="txtPrompt">Ask CodeQuest AI</label>
                            <asp:TextBox ID="txtPrompt" runat="server" ClientIDMode="Static" CssClass="prompt-input" TextMode="MultiLine" Rows="4" MaxLength="2000" placeholder="Ask about your current lesson..." />
                            <div class="composer-footer"><span>Keep questions focused on your learning.</span><asp:Button ID="btnAsk" runat="server" CssClass="ai-button" Text="Ask CodeQuest AI" OnClick="btnAsk_Click" /></div>
                        </div>
                    </section>
                </div>
            </asp:Panel>
        </main>

        <footer class="site-footer">
            <span>&copy; 2026 CodeQuest</span>
            <span>Learn &middot; Practise &middot; Build</span>
        </footer>
    </form>
    <script>
        function setPrompt(value) {
            var input = document.getElementById('txtPrompt');
            if (input) { input.value = value; input.focus(); }
        }
    </script>
    <script src="../../Content/codequest-responsive-nav.js?v=50"></script>
</body>
</html>
