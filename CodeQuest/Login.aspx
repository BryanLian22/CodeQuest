<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="CodeQuest.Login" UnobtrusiveValidationMode="None" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <meta name="description" content="Log in to CodeQuest and continue your coding journey." />
    <title>Log in | CodeQuest</title>
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous" />
    <link href="https://fonts.googleapis.com/css2?family=DM+Sans:wght@400;500;600;700&amp;family=Space+Grotesk:wght@600;700&amp;display=swap" rel="stylesheet" />
    <link href="Content/codequest-auth.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <header class="site-header">
            <a class="brand" href="Guest.aspx" aria-label="CodeQuest home">
                <img class="brand-logo" src="Content/Images/CodeQuest_logo.png" alt="CodeQuest" />
            </a>

            <nav class="main-nav" aria-label="Main navigation">
                <a href="Guest.aspx">Home</a>
                <a href="Features/Public/Courses.aspx">Courses</a>
                <a href="Features/Public/Tutorials.aspx">Tutorials</a>
                <a href="Guest.aspx#about">About</a>
            </nav>

            <a class="header-cta" href="Register.aspx">Get Started</a>
        </header>

        <main class="login-page">
            <section class="welcome-panel" aria-labelledby="welcomeTitle">
                <p class="eyebrow"><span></span> Learn &middot; Practise &middot; Build</p>
                <h1 id="welcomeTitle">Welcome back to your <em>coding journey.</em></h1>
                <p class="welcome-copy">
                    Continue learning HTML, CSS and JavaScript through guided lessons,
                    practical exercises and real projects.
                </p>

                <div class="code-preview" aria-label="CodeQuest code preview">
                    <div class="preview-toolbar">
                        <div class="window-dots" aria-hidden="true"><i></i><i></i><i></i></div>
                        <span>continue-learning.js</span>
                        <span class="preview-status">Ready</span>
                    </div>
                    <pre><code><span class="line-number">01</span> <span class="code-blue">const</span> learner = <span class="code-green">"you"</span>;
<span class="line-number">02</span> <span class="code-blue">const</span> nextStep = <span class="code-green">"build"</span>;
<span class="line-number">03</span>
<span class="line-number">04</span> learner.<span class="code-purple">continue</span>(nextStep);<span class="cursor"></span></code></pre>
                    <div class="progress-row">
                        <span>Course progress</span>
                        <div class="progress-track"><span></span></div>
                        <strong>65%</strong>
                    </div>
                </div>

                <div class="trust-row">
                    <span><strong>25K+</strong> students learning</span>
                    <span><strong>120+</strong> interactive lessons</span>
                    <span><strong>98%</strong> beginner friendly</span>
                </div>
            </section>

            <section class="login-card" aria-labelledby="loginTitle">
                <div class="card-heading">
                    <span class="card-icon" aria-hidden="true">CQ</span>
                    <div>
                        <p>Welcome back</p>
                        <h2 id="loginTitle">Log in to CodeQuest</h2>
                    </div>
                </div>
                <p class="card-subtitle">Continue where you left off.</p>

                <asp:Panel ID="pnlMessage" runat="server" CssClass="form-message" Visible="false" role="alert">
                    <asp:Label ID="lblMessage" runat="server" />
                </asp:Panel>

                <asp:Button
                    ID="btnGoogle"
                    runat="server"
                    CssClass="google-button"
                    Text="G   Continue with Google"
                    CausesValidation="false"
                    OnClick="btnGoogle_Click" />

                <div class="divider"><span>or log in with email</span></div>

                <div class="field-group">
                    <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail" Text="Email address" />
                    <div class="input-wrap">
                        <span class="input-icon" aria-hidden="true">@</span>
                        <asp:TextBox
                            ID="txtEmail"
                            runat="server"
                            CssClass="form-input"
                            TextMode="Email"
                            MaxLength="150"
                            autocomplete="email"
                            placeholder="name@example.com" />
                    </div>
                    <asp:RequiredFieldValidator
                        ID="rfvEmail"
                        runat="server"
                        ControlToValidate="txtEmail"
                        CssClass="validation-message"
                        ErrorMessage="Please enter your email address."
                        Display="Dynamic"
                        ValidationGroup="LoginValidation" />
                    <asp:RegularExpressionValidator
                        ID="revEmail"
                        runat="server"
                        ControlToValidate="txtEmail"
                        CssClass="validation-message"
                        ErrorMessage="Please enter a valid email address."
                        ValidationExpression="^[^\s@]+@[^\s@]+\.[^\s@]+$"
                        Display="Dynamic"
                        ValidationGroup="LoginValidation" />
                </div>

                <div class="field-group">
                    <div class="label-row">
                        <asp:Label ID="lblPassword" runat="server" AssociatedControlID="txtPassword" Text="Password" />
                        <a href="ForgotPassword.aspx">Forgot password?</a>
                    </div>
                    <div class="input-wrap">
                        <span class="input-icon lock-icon" aria-hidden="true">*</span>
                        <asp:TextBox
                            ID="txtPassword"
                            runat="server"
                            ClientIDMode="Static"
                            CssClass="form-input password-input"
                            TextMode="Password"
                            MaxLength="100"
                            autocomplete="current-password"
                            placeholder="Enter your password" />
                        <button type="button" id="togglePassword" class="password-toggle" aria-label="Show password" aria-pressed="false">
                            <span aria-hidden="true">Show</span>
                        </button>
                    </div>
                    <asp:RequiredFieldValidator
                        ID="rfvPassword"
                        runat="server"
                        ControlToValidate="txtPassword"
                        CssClass="validation-message"
                        ErrorMessage="Please enter your password."
                        Display="Dynamic"
                        ValidationGroup="LoginValidation" />
                </div>

                <div class="form-options">
                    <asp:CheckBox ID="chkRememberMe" runat="server" Text="Remember me" />
                </div>

                <asp:Button
                    ID="btnLogin"
                    runat="server"
                    CssClass="login-button"
                    Text="Log in -&gt;"
                    ValidationGroup="LoginValidation"
                    OnClick="btnLogin_Click" />

                <p class="register-prompt">New to CodeQuest? <a href="Register.aspx">Create an account</a></p>

                <div class="demo-box">
                    <div>
                        <span>Demo learner</span>
                        <code>learner@codequest.io</code>
                        <small>Password: Learner123!</small>
                    </div>
                    <div>
                        <span>Demo admin</span>
                        <code>admin@codequest.io</code>
                        <small>Password: Admin123!</small>
                    </div>
                </div>
            </section>
        </main>

        <footer class="site-footer">
            <span>&copy; 2026 CodeQuest</span>
            <span>Learn &middot; Practise &middot; Build</span>
        </footer>
    </form>

    <script>
        (function () {
            var password = document.getElementById('txtPassword');
            var toggle = document.getElementById('togglePassword');

            if (!password || !toggle) return;

            toggle.addEventListener('click', function () {
                var showPassword = password.type === 'password';
                password.type = showPassword ? 'text' : 'password';
                toggle.setAttribute('aria-pressed', showPassword ? 'true' : 'false');
                toggle.setAttribute('aria-label', showPassword ? 'Hide password' : 'Show password');
                toggle.querySelector('span').textContent = showPassword ? 'Hide' : 'Show';
                password.focus();
            });
        }());
    </script>
</body>
</html>
