<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="CodeQuest.ForgotPassword" UnobtrusiveValidationMode="None" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Forgot password | CodeQuest</title>
    <link href="Content/codequest-auth.css" rel="stylesheet" />
    <link href="Content/codequest-reset.css" rel="stylesheet" />
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
            <a class="header-cta" href="Login.aspx">Log in</a>
        </header>

        <main class="reset-page">
            <section class="reset-card" aria-labelledby="resetTitle">
                <div class="card-heading">
                    <span class="card-icon" aria-hidden="true">CQ</span>
                    <div><p>Account recovery</p><h1 id="resetTitle">Reset your password.</h1></div>
                </div>
                <p class="card-subtitle">Enter the email address on your CodeQuest account and we will prepare a secure reset link.</p>

                <asp:Panel ID="pnlMessage" runat="server" CssClass="form-message" Visible="false" role="status">
                    <asp:Label ID="lblMessage" runat="server" />
                </asp:Panel>

                <div class="field-group">
                    <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail" Text="Email address" />
                    <div class="input-wrap">
                        <span class="input-icon" aria-hidden="true">@</span>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-input" TextMode="Email" MaxLength="254" autocomplete="email" placeholder="name@example.com" />
                    </div>
                </div>

                <asp:Button ID="btnRequestReset" runat="server" CssClass="login-button" Text="Send reset link" OnClick="btnRequestReset_Click" />

                <asp:Panel ID="pnlLocalReset" runat="server" CssClass="local-reset" Visible="false">
                    <strong>Local development link</strong>
                    <span>Email delivery is not configured, so use this one-time link to test the flow.</span>
                    <asp:HyperLink ID="lnkLocalReset" runat="server" Target="_self" Text="Open password reset" />
                    <small>Expires at <asp:Label ID="lblExpiry" runat="server" />.</small>
                </asp:Panel>

                <p class="register-prompt"><a href="Login.aspx">&larr; Back to log in</a></p>
            </section>
        </main>

        <footer class="site-footer">
            <span>&copy; 2026 CodeQuest</span>
            <span>Learn &middot; Practise &middot; Build</span>
        </footer>
    </form>
</body>
</html>
