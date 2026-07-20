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
