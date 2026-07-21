<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="CodeQuest.ResetPassword" UnobtrusiveValidationMode="None" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Choose a new password | CodeQuest</title>
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
                    <div><p>Account recovery</p><h1 id="resetTitle">Choose a new password.</h1></div>
                </div>
                <p class="card-subtitle">Use a strong password so your courses, progress and profile stay protected.</p>

                <asp:Panel ID="pnlMessage" runat="server" CssClass="form-message" Visible="false" role="alert">
                    <asp:Label ID="lblMessage" runat="server" />
                </asp:Panel>

                <asp:Panel ID="pnlResetForm" runat="server" Visible="false">
                    <div class="field-group">
                        <asp:Label ID="lblNewPassword" runat="server" AssociatedControlID="txtNewPassword" Text="New password" />
                        <asp:TextBox ID="txtNewPassword" runat="server" CssClass="form-input reset-input" TextMode="Password" MaxLength="100" autocomplete="new-password" placeholder="Create a new password" />
                        <small class="reset-help">At least 8 characters with uppercase, lowercase, number and symbol.</small>
                    </div>
                    <div class="field-group">
                        <asp:Label ID="lblConfirmPassword" runat="server" AssociatedControlID="txtConfirmPassword" Text="Confirm new password" />
                        <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-input reset-input" TextMode="Password" MaxLength="100" autocomplete="new-password" placeholder="Repeat your new password" />
                    </div>
                    <asp:Button ID="btnResetPassword" runat="server" CssClass="login-button" Text="Save new password" OnClick="btnResetPassword_Click" />
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
