<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="CodeQuest.Features.Learner.Profile" %>
<!-- Page purpose: Lets a learner view and update personal account and biography information. -->

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>My profile | CodeQuest</title>
    <link href="../../Content/codequest-home.css?v=50" rel="stylesheet" />
    <link href="../../Content/codequest-profile.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <header class="site-header">
            <a class="brand" href="../../Guest.aspx" aria-label="CodeQuest home">
                <img class="brand-logo" src="../../Content/Images/CodeQuest_logo.png" alt="CodeQuest" />
            </a>
            <nav class="main-nav" aria-label="Learner navigation">
                <a href="../../LearnerDashboard.aspx">Dashboard</a>
                <a href="Courses.aspx">Courses</a>
                <a href="../../LearnerDashboard.aspx#myLearning">My learning</a>
                <a href="../AI/Assistant.aspx">AI assistant</a>
                <a class="active" href="Profile.aspx">Profile</a>
                <a href="../Support/Tickets.aspx">Support</a>
            </nav>
            <div class="header-actions">
                <a class="login-link" href="../Billing/Plans.aspx">Manage plan</a>
                <a class="header-cta" href="../../Login.aspx?logout=1">Sign out</a>
            </div>
        </header>

        <main class="profile-page">
            <section class="profile-heading">
                <div>
                    <p class="eyebrow"><span></span> Learner profile</p>
                    <h1>Your learning identity.</h1>
                    <p>Keep your public name and biography current, or securely replace the password used for your CodeQuest account.</p>
                </div>
                <div class="profile-badge"><span>Current plan</span><strong><asp:Label ID="lblPlan" runat="server" /></strong><a href="../Billing/Plans.aspx">Manage plan &rarr;</a></div>
            </section>

            <asp:Panel ID="pnlSuccess" runat="server" CssClass="profile-message success" Visible="false" role="status">
                <asp:Label ID="lblSuccess" runat="server" />
            </asp:Panel>
            <asp:Panel ID="pnlError" runat="server" CssClass="profile-message error" Visible="false" role="alert">
                <asp:Label ID="lblError" runat="server" />
            </asp:Panel>

            <div class="profile-grid">
                <section class="profile-card identity-card">
                    <div class="card-heading">
                        <div><p class="section-kicker">Account details</p><h2>Edit your profile.</h2></div>
                        <span class="account-number">USER-<asp:Label ID="lblUserID" runat="server" /></span>
                    </div>

                    <label class="field-label" for="txtUsername">Username</label>
                    <asp:TextBox ID="txtUsername" runat="server" CssClass="profile-input" MaxLength="30" autocomplete="username" />
                    <span class="field-help">3-30 letters, numbers or underscores.</span>

                    <label class="field-label" for="lblEmail">Email address</label>
                    <div class="readonly-field"><asp:Label ID="lblEmail" runat="server" /></div>
                    <span class="field-help">Contact Support to change Email.</span>

                    <label class="field-label" for="txtBio">Biography</label>
                    <asp:TextBox ID="txtBio" runat="server" CssClass="profile-input profile-textarea" TextMode="MultiLine" Rows="7" MaxLength="1000" placeholder="Tell the CodeQuest community what you are learning." />

                    <asp:Button ID="btnSaveProfile" runat="server" CssClass="profile-button" Text="Save profile" OnClick="btnSaveProfile_Click" />
                </section>

                <section class="profile-card security-card">
                    <div class="card-heading">
                        <div><p class="section-kicker">Security</p><h2>Change your password.</h2></div>
                    </div>
                    <p class="card-intro">Your new password is salted and hashed before it is stored. CodeQuest never saves the readable password.</p>

                    <label class="field-label" for="txtCurrentPassword">Current password</label>
                    <asp:TextBox ID="txtCurrentPassword" runat="server" CssClass="profile-input" TextMode="Password" autocomplete="current-password" />

                    <label class="field-label" for="txtNewPassword">New password</label>
                    <asp:TextBox ID="txtNewPassword" runat="server" CssClass="profile-input" TextMode="Password" MaxLength="100" autocomplete="new-password" />
                    <span class="field-help">At least 8 characters with uppercase, lowercase, number and symbol.</span>

                    <label class="field-label" for="txtConfirmPassword">Confirm new password</label>
                    <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="profile-input" TextMode="Password" MaxLength="100" autocomplete="new-password" />

                    <asp:Button ID="btnChangePassword" runat="server" CssClass="profile-button" Text="Update password" OnClick="btnChangePassword_Click" />

                    <div class="connection-summary">
                        <div><span>Role</span><strong><asp:Label ID="lblRole" runat="server" /></strong></div>
                        <div><span>Google account</span><strong><asp:Label ID="lblGoogleStatus" runat="server" /></strong></div>
                    </div>
                </section>
            </div>
        </main>

        <footer class="site-footer">
            <span>&copy; 2026 CodeQuest</span>
            <span>Learn &middot; Practise &middot; Build</span>
        </footer>
    </form>
    <script src="../../Content/codequest-responsive-nav.js?v=50"></script>
</body>
</html>
