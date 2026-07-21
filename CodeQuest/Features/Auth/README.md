# Auth

Implement in this order:

1. Login against `dbo.User`.
2. Register a learner in `dbo.User` with a salted password hash.
3. Forgot-password tokens in `dbo.Token`.
4. Google sign-in using the server-side OAuth authorization-code flow and `google_id`.
5. Role and plan checks for `Learner`, `Admin`, `Basic` and `Premium`.

The current root `Login.aspx` and `Register.aspx` are the working visual
prototype. `ForgotPassword.aspx` issues a 30-minute one-time reset token, and
`ResetPassword.aspx` validates it before replacing the password in a SQL
transaction. The raw token is used only in the URL; `dbo.Token.token` stores
its SHA-256 digest.

For real email delivery, add `CodeQuestSmtpHost`, `CodeQuestResetFromEmail`
and optional `CodeQuestSmtpPort`, `CodeQuestSmtpSsl`,
`CodeQuestSmtpUsername` and `CodeQuestSmtpPassword` values to Web.config's
`appSettings`. Without these settings, the local development reset link is
shown after a matching account request.

Google sign-in uses OpenID Connect scopes `openid email profile`. Add
`CodeQuestGoogleClientId`, `CodeQuestGoogleClientSecret` and the exact
`CodeQuestGoogleRedirectUri` to Web.config's `appSettings`. Register the same
redirect URI in the Google Cloud Console OAuth Web application. The callback
validates the state value, verifies the Google email, links an existing
`dbo.User` by email when safe, or creates a Basic learner with a random
unusable local password. Google access tokens are not stored.
