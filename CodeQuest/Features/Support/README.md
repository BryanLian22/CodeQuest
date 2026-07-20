# Support

Create a `Ticket` for the signed-in user and store administrator or learner
responses in `Reply`. Ticket status should move through `Open`, `In Progress`,
`Resolved` and `Closed`.
# Support

`Tickets.aspx` is the learner-facing Contact Us page. It requires a signed-in
learner, creates tickets in `dbo.Ticket`, shows ticket history, and lets the
learner reply while a ticket is open.

`Features/Admin/Support.aspx` is the administrator support desk. It lists all
tickets, shows the conversation, lets an administrator reply, and manages the
`Open`, `In Progress`, `Resolved` and `Closed` statuses.

The root `Contact.aspx` route sends guests to Login, learners to their ticket
history, and administrators to the support desk.
