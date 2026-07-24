# Learner

The learner area must read the signed-in user from Session, then use
`Enrollment` to show courses. Chapter and quiz pages use the content chain:

`Course -> Module -> Chapter -> Quiz -> QuizAns`

`Database/Progress_Extension.sql` adds the two small application tables that
extend the ERD for saved learner work (`ChapterProgress` and `QuizAttempt`) and
adds `Tutorial.category` for the public HTML/CSS/JavaScript catalogue. The
database initializer applies it automatically on first use.

The root `LearnerDashboard.aspx` is now database-backed. It reads the signed-in
user's `UserID` from Session and loads enrollments through `EnrollmentRepository`.

`Profile.aspx` loads the current learner from `dbo.User`. Learners can update
their username and biography, while their email, role, plan and Google
connection status are shown as account information. Password changes require
the current password and replace `dbo.User.password` with a newly salted PBKDF2
hash. The page never displays a stored password hash.

`Enroll.aspx` is the course-specific enrolment step. It loads a course from
`dbo.Course`, returns guests to Login and then back to the selected course,
allows Basic learners to enrol in Beginner courses, and requires Premium for
Intermediate and Advanced courses. Successful enrolments are inserted into
`dbo.Enrollment` and shown on the learner dashboard.

`Course.aspx` is the first enrolled-learning page. It checks the current
learner's `Enrollment` record and lists published `Module` and `Chapter` rows
from the course content chain.

`Chapter.aspx` checks the learner's `Enrollment` record before listing the
matching chapter lesson. Chapters without quizzes complete when viewed.
Chapters with quizzes complete only after a score of 75% or higher. When every
chapter in a module is complete, the learner course page labels that module
`COMPLETED`. Public
`Tutorial.aspx` is separate and reads `Tutorial` and `Exercise` rows without
requiring login. `Quiz.aspx` is learner-only, checks enrollment again, records
each selected answer, and marks the chapter complete only after the passing
threshold is reached. The dashboard reads completed chapters and the saved
quiz average.
