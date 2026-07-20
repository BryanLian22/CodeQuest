# Public pages

Guests can browse the public `Tutorial` and `Exercise` content without
logging in. Login is required to create an `Enrollment`, open learner-only
`Course -> Module -> Chapter` content and save progress.

The first database-driven page is `Features/Public/Courses.aspx`. It reads
from `dbo.Course` through `CourseRepository`.

Recommended pages: `Guest.aspx`, `Courses.aspx`, `CourseDetails.aspx`,
`Tutorials.aspx`, `TutorialDetails.aspx`, `About.aspx`, and the role-aware
root `Contact.aspx` support entry point.
`Tutorials.aspx` is the guest-accessible tutorial catalogue. It reads published
`Tutorial` and `Exercise` rows for HTML, CSS and JavaScript and links to free
tutorial pages. Category chips filter the database `Tutorial.category` field.
Guests can read tutorials and complete exercises; login is reserved for saved
progress, completion records and quizzes.
