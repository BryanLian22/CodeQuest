# CodeQuest feature structure

The current `.aspx` pages remain at the project root so the working URLs do
not change. New pages and code should be added under the feature folders below.
The SQL tables used by each feature are listed so the UI and backend stay
aligned with the supplied ERD.

| Feature folder | First pages | ERD tables |
|---|---|---|
| `Auth` | Login, Register, Forgot Password, Google Sign-in | `User`, `Token` |
| `Public` | Guest Home, Courses, Tutorials, About, Contact | `Course`, `Module`, `Chapter`, `Tutorial`, `Exercise` |
| `Learner` | Dashboard, My Learning, Chapter, Quiz, Profile | `User`, `Enrollment`, `Course`, `Module`, `Chapter`, `Quiz`, `QuizAns` |
| `Admin` | Overview, Courses, Modules & Chapters, Tutorials & Exercises, Quizzes & Answers, Users | All content tables and `User` |
| `Billing` | Plans, Subscription, Course Payment, Payment Result | `Subscription`, `Payment`, `Enrollment` |
| `Support` | Contact Us, Ticket History, Admin Replies | `Ticket`, `Reply`, `User` |
| `AI` | AI Assistant, Conversation History | Current user/chapter context; add conversation tables later |

`Data/DbConnectionFactory.cs` and `Data/Repositories` contain the first
database access layer. Add new repositories instead of putting SQL queries
inside `.aspx.cs` page event handlers.
