<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="CodeQuest.Register" UnobtrusiveValidationMode="None" %>
<!-- Page purpose: Creates a learner account through email registration or Google OAuth. -->

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <meta name="description" content="Create a CodeQuest learner account." />
    <title>Create account | CodeQuest</title>
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous" />
    <link href="https://fonts.googleapis.com/css2?family=DM+Sans:wght@400;500;600;700&amp;family=Space+Grotesk:wght@600;700&amp;display=swap" rel="stylesheet" />
    <link href="Content/codequest-auth.css?v=50" rel="stylesheet" />
    <link href="Content/codequest-register.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <header class="site-header">
            <a class="brand" href="Guest.aspx" aria-label="CodeQuest home">
                <img class="brand-logo" src="Content/Images/CodeQuest_logo.png" alt="CodeQuest" />
            </a>
            <nav class="main-nav" aria-label="Main navigation">
                <a href="Guest.aspx">Home</a>
                <a href="Guest.aspx#courses">Courses</a>
                <a href="Guest.aspx#tutorials">Tutorials</a>
                <a href="Guest.aspx#about">About</a>
                <a href="Contact.aspx">Contact Us</a>
            </nav>
            <a class="header-cta" href="Login.aspx">Login</a>
        </header>

        <main class="login-page register-page">
            <section class="welcome-panel register-welcome" aria-labelledby="welcomeTitle">
                <p class="eyebrow"><span></span> Your journey starts here</p>
                <h1 id="welcomeTitle">Create your account.<br /><em>Start building.</em></h1>
                <p class="welcome-copy">Unlock full tutorials, exercises, quizzes and saved course progress with your free learner account.</p>

                <div class="benefit-list">
                    <article><span>01</span><div><strong>Learn step by step</strong><p>Follow structured modules from beginner to advanced.</p></div></article>
                    <article><span>02</span><div><strong>Practise as you learn</strong><p>Complete exercises and quizzes inside each lesson.</p></div></article>
                    <article><span>03</span><div><strong>Save your progress</strong><p>Continue from your latest chapter on any visit.</p></div></article>
                </div>
            </section>

            <section class="login-card register-card" aria-labelledby="registerTitle">
                <div class="card-heading">
                    <span class="card-icon" aria-hidden="true">CQ</span>
                    <div>
                        <p>Join CodeQuest</p>
                        <h2 id="registerTitle">Create your account</h2>
                    </div>
                </div>
                <p class="card-subtitle">Start your coding journey today.</p>

                <asp:ValidationSummary
                    ID="vsRegister"
                    runat="server"
                    CssClass="form-message error validation-summary"
                    HeaderText="Please check the following:"
                    ValidationGroup="RegisterValidation" />

                <asp:Panel ID="pnlMessage" runat="server" CssClass="form-message" Visible="false" role="alert">
                    <asp:Label ID="lblMessage" runat="server" />
                </asp:Panel>

                <asp:Button
                    ID="btnGoogleRegister"
                    runat="server"
                    CssClass="google-button"
                    Text="G   Sign up with Google"
                    CausesValidation="false"
                    OnClick="btnGoogleRegister_Click" />

                <div class="divider"><span>or register with email</span></div>

                <div class="field-group">
                    <asp:Label ID="lblUsername" runat="server" AssociatedControlID="txtUsername" Text="Username" />
                    <div class="input-wrap">
                        <span class="input-icon" aria-hidden="true">#</span>
                        <asp:TextBox ID="txtUsername" runat="server" CssClass="form-input" MaxLength="30" autocomplete="username" placeholder="Choose a username" />
                    </div>
                    <asp:RequiredFieldValidator ID="rfvUsername" runat="server" ControlToValidate="txtUsername" CssClass="validation-message" ErrorMessage="Username is required." Display="Dynamic" ValidationGroup="RegisterValidation" />
                    <asp:RegularExpressionValidator ID="revUsername" runat="server" ControlToValidate="txtUsername" CssClass="validation-message" ErrorMessage="Use 3-30 letters, numbers or underscores." ValidationExpression="^[A-Za-z0-9_]{3,30}$" Display="Dynamic" ValidationGroup="RegisterValidation" />
                </div>

                <div class="field-group">
                    <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail" Text="Email address" />
                    <div class="input-wrap">
                        <span class="input-icon" aria-hidden="true">@</span>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-input" TextMode="Email" MaxLength="150" autocomplete="email" placeholder="name@example.com" />
                    </div>
                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" CssClass="validation-message" ErrorMessage="Email address is required." Display="Dynamic" ValidationGroup="RegisterValidation" />
                    <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail" CssClass="validation-message" ErrorMessage="Enter a valid email address." ValidationExpression="^[^\s@]+@[^\s@]+\.[^\s@]+$" Display="Dynamic" ValidationGroup="RegisterValidation" />
                </div>

                <div class="password-grid">
                    <div class="field-group">
                        <asp:Label ID="lblPassword" runat="server" AssociatedControlID="txtPassword" Text="Password" />
                        <div class="input-wrap">
                            <span class="input-icon lock-icon" aria-hidden="true">*</span>
                            <asp:TextBox ID="txtPassword" runat="server" ClientIDMode="Static" CssClass="form-input password-input" TextMode="Password" MaxLength="100" autocomplete="new-password" placeholder="Create password" />
                            <button type="button" class="password-toggle password-control" data-password-target="txtPassword" aria-label="Show password"><span>Show</span></button>
                        </div>
                        <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword" CssClass="validation-message" ErrorMessage="Password is required." Display="Dynamic" ValidationGroup="RegisterValidation" />
                        <asp:RegularExpressionValidator ID="revPassword" runat="server" ControlToValidate="txtPassword" CssClass="validation-message" ErrorMessage="Password must contain 8+ characters, uppercase, lowercase, number and symbol." ValidationExpression="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,100}$" Display="Dynamic" ValidationGroup="RegisterValidation" />
                    </div>

                    <div class="field-group">
                        <asp:Label ID="lblConfirmPassword" runat="server" AssociatedControlID="txtConfirmPassword" Text="Confirm password" />
                        <div class="input-wrap">
                            <span class="input-icon lock-icon" aria-hidden="true">*</span>
                            <asp:TextBox ID="txtConfirmPassword" runat="server" ClientIDMode="Static" CssClass="form-input password-input" TextMode="Password" MaxLength="100" autocomplete="new-password" placeholder="Repeat password" />
                            <button type="button" class="password-toggle password-control" data-password-target="txtConfirmPassword" aria-label="Show password"><span>Show</span></button>
                        </div>
                        <asp:RequiredFieldValidator ID="rfvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword" CssClass="validation-message" ErrorMessage="Please confirm your password." Display="Dynamic" ValidationGroup="RegisterValidation" />
                        <asp:CompareValidator ID="cvPasswords" runat="server" ControlToValidate="txtConfirmPassword" ControlToCompare="txtPassword" CssClass="validation-message" ErrorMessage="Passwords do not match." Display="Dynamic" ValidationGroup="RegisterValidation" />
                    </div>
                </div>

                <div class="password-strength" id="passwordStrength" aria-live="polite">
                    <div><span></span><span></span><span></span><span></span></div>
                    <small>Use uppercase, lowercase, number and symbol.</small>
                </div>

                <div class="terms-row">
                    <asp:CheckBox ID="chkTerms" runat="server" Text="I agree to the CodeQuest terms and privacy policy" />
                    <asp:CustomValidator ID="cvTerms" runat="server" CssClass="validation-message" ErrorMessage="You must accept the terms and privacy policy." Display="Dynamic" ValidationGroup="RegisterValidation" OnServerValidate="cvTerms_ServerValidate" />
                </div>

                <asp:Button ID="btnRegister" runat="server" CssClass="login-button" Text="Create account -&gt;" ValidationGroup="RegisterValidation" OnClick="btnRegister_Click" />
                <p class="register-prompt">Already registered? <a href="Login.aspx">Log in instead</a></p>
            </section>
        </main>

        <footer class="site-footer">
            <span>&copy; 2026 CodeQuest</span>
            <span>Learn &middot; Practise &middot; Build</span>
        </footer>
    </form>

    <script>
        (function () {
            Array.prototype.forEach.call(document.querySelectorAll('.password-control'), function (button) {
                button.addEventListener('click', function () {
                    var input = document.getElementById(button.getAttribute('data-password-target'));
                    var show = input.type === 'password';
                    input.type = show ? 'text' : 'password';
                    button.querySelector('span').textContent = show ? 'Hide' : 'Show';
                    button.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
                });
            });

            var password = document.getElementById('txtPassword');
            var meter = document.getElementById('passwordStrength');

            password.addEventListener('input', function () {
                var value = password.value;
                var score = 0;
                if (value.length >= 8) score++;
                if (/[a-z]/.test(value) && /[A-Z]/.test(value)) score++;
                if (/\d/.test(value)) score++;
                if (/[^A-Za-z0-9]/.test(value)) score++;

                meter.setAttribute('data-score', score);
                meter.querySelector('small').textContent = score === 4
                    ? 'OK - Your password meets the security requirements.'
                    : 'Use uppercase, lowercase, number and symbol.';
            });
        }());
    </script>
    <script src="Content/codequest-responsive-nav.js?v=50"></script>
</body>
</html>
