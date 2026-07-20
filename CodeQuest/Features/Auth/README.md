# Auth

Implement in this order:

1. Login against `dbo.User`.
2. Register a learner in `dbo.User` with a salted password hash.
3. Forgot-password tokens in `dbo.Token`.
4. Google sign-in using `google_id`.
5. Role and plan checks for `Learner`, `Admin`, `Basic` and `Premium`.

The current root `Login.aspx` and `Register.aspx` are the working visual
prototype. Move them here only after the project has been tested with the
database connection.
