# Admin

Admin pages manage the content records in the same order as the ERD:

`Course -> Module -> Chapter -> Tutorial/Exercise/Quiz/QuizAns`

Every create, edit, delete and publish operation should be protected by the
Admin role stored in `dbo.User.role`.

`AdminDashboard.aspx` is the first protected admin workspace. It loads content
counts and recent courses from CodeQuestDB and previews the learner course
workspace. `Features/Admin/Content.aspx` now provides the first authoring
slice: Admins can create courses, add modules and chapters, and publish or
archive modules. Existing courses, modules and chapters can be loaded back
into their forms and edited without creating duplicate records.

`Features/Admin/Lessons.aspx` adds tutorial, guest exercise and chapter quiz
creation, including answer choices and publishing status. Existing tutorials
and exercises can be edited, while quiz editing updates its description,
question, correct answer and answer choices together in one database
transaction. The chapter quiz selector follows the content hierarchy so an
administrator first chooses a course, then one of its modules, and finally a
chapter from that module.

Administrators can keep the Admin navigation while previewing courses and
tutorials. Direct test links in Content studio and Lesson library can open
draft, review, published or archived content without writing learner progress.
Edit, preview, publish, review and archive actions use distinct outlined
buttons so the available workflow controls are easy to identify.

User Management allows an administrator to change a learner's email address
after validating its format and checking that it is not used by another
account. Administrator email addresses remain protected.

`Features/Admin/Users.aspx` is the protected user-management directory. It
searches `dbo.User` by username or email, counts each account's enrolments and
support tickets, and lets an administrator update the checked `role` and
`plan` values. It does not expose password hashes or perform destructive user
deletion. The current administrator cannot demote their own account, and the
last remaining administrator cannot be demoted.
