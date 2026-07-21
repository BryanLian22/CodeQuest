# Admin

Admin pages manage the content records in the same order as the ERD:

`Course -> Module -> Chapter -> Tutorial/Exercise/Quiz/QuizAns`

Every create, edit, delete and publish operation should be protected by the
Admin role stored in `dbo.User.role`.

`AdminDashboard.aspx` is the first protected admin workspace. It loads content
counts and recent courses from CodeQuestDB and previews the learner course
workspace. `Features/Admin/Content.aspx` now provides the first authoring
slice: Admins can create courses, add modules and chapters, and publish or
archive modules. `Features/Admin/Lessons.aspx` adds tutorial, guest exercise
and chapter quiz creation, including answer choices and publishing status.

`Features/Admin/Users.aspx` is the protected user-management directory. It
searches `dbo.User` by username or email, counts each account's enrolments and
support tickets, and lets an administrator update the checked `role` and
`plan` values. It does not expose password hashes or perform destructive user
deletion. The current administrator cannot demote their own account, and the
last remaining administrator cannot be demoted.
