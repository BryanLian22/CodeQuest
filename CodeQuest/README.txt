CODEQUEST LOGIN PAGE - ASP.NET WEB FORMS (.NET FRAMEWORK 4.7.2)
================================================================

FILES
-----
Login.aspx                         Login page markup
Login.aspx.cs                      C# code-behind and temporary demo login
Login.aspx.designer.cs             Web Forms control declarations
Content/codequest-auth.css         Responsive CodeQuest styling
Guest.aspx                         Public CodeQuest home page
Guest.aspx.cs                      Guest page code-behind
Guest.aspx.designer.cs             Guest page control declarations
Register.aspx                      Registration page markup
Register.aspx.cs                   Registration logic and password hashing
Register.aspx.designer.cs          Registration control declarations
Content/codequest-home.css         Responsive guest-home styling
Content/codequest-register.css     Registration-specific styling
Content/Images/CodeQuest_logo.png  Shared CodeQuest website logo
Database/CodeQuest_Database.sql   ERD-based SQL Server schema
Database/Progress_Extension.sql  Progress, quiz attempts and tutorial categories
Database/Seed_Demo_Content.sql    Optional courses and learning content
Models/                            C# records matching ERD entities
Data/                              SQL Server connection and repositories
Features/                          Feature folders and ERD mapping
Features/Public/Courses.aspx      First database-driven public page
Features/Public/Tutorials.aspx    Guest-accessible tutorial catalogue
Features/Learner/Enroll.aspx       Course-specific enrolment step
Features/Learner/Course.aspx       Enrolled course and published modules
Features/Learner/Chapter.aspx      Chapter tutorial and practice page
Features/Learner/Quiz.aspx         Enrolled learner checkpoint quiz
Features/Billing/Plans.aspx        Basic/Premium plans and demo checkout
LearnerDashboard.aspx              Database-backed learner dashboard
AdminDashboard.aspx                Protected database-backed admin overview
Features/Admin/Content.aspx        Admin course, module and chapter authoring
Features/Admin/Lessons.aspx        Admin tutorial, exercise and quiz authoring
Features/Support/Tickets.aspx      Learner ticket history and replies
Features/Admin/Support.aspx        Admin ticket inbox, replies and status control
Contact.aspx                       Role-aware Contact Us entry point

The learner plan page follows the prototype's subscription model: Basic is
RM0 forever and unlocks beginner courses, while Premium is RM29/month and
unlocks intermediate and advanced courses. The Premium action is a simulated
local checkout for this academic prototype. It writes an Active row to
dbo.Subscription, a Completed row to dbo.Payment and updates dbo.User.[plan]
inside one SQL transaction; it does not collect card details.

Contact Us now uses the ERD Ticket and Reply tables. Learners can create a
ticket by category, view a ticket reference and reply to open tickets.
Administrators can review every ticket, respond and set Open, In Progress,
Resolved or Closed status from Features/Admin/Support.aspx.

HOW TO ADD THE FILES TO YOUR VISUAL STUDIO PROJECT
--------------------------------------------------
1. Close Login.aspx in Visual Studio if it is currently open.
2. Copy all .aspx, .aspx.cs and .aspx.designer.cs files into the CodeQuest
   project folder, replacing the blank Login and Guest files.
3. Create a Content folder inside CodeQuest if it does not exist.
4. Copy all three CSS files into the Content folder, including the Images
   subfolder and CodeQuest_logo.png.
5. Copy the Database, Models, Data and Features folders into the project.
6. Keep LearnerDashboard.aspx and AdminDashboard.aspx in the project root.
7. In Solution Explorer, click "Show All Files" if the copied files do not
   appear, then right-click each file and choose "Include In Project".
8. Right-click Login.aspx and choose "Set as Start Page".
9. Press Ctrl+F5 to run the website.

IF REGISTER SHOWS "COULD NOT LOAD TYPE CODEQUEST.REGISTER"
--------------------------------------------------------------
This means Register.aspx.cs is not being compiled by the project, or its
namespace/class name does not match the page directive. In Solution Explorer:

1. Click "Show All Files".
2. Right-click Register.aspx.cs and Register.aspx.designer.cs and choose
   "Include In Project".
3. Select Register.aspx.cs and Register.aspx.designer.cs one at a time and
   confirm Properties > Build Action is "Compile".
4. Select Register.aspx and confirm Properties > Build Action is "Content".
5. Right-click Register.aspx and choose "Convert to Web Application".
6. Choose Build > Clean Solution, then Build > Rebuild Solution.

The files must contain `namespace CodeQuest` and `public partial class
Register`, matching `Inherits="CodeQuest.Register"` on line 1 of Register.aspx.
If your project uses a different namespace, change the namespace in both C#
files and the Inherits value together.

VALIDATION COMPATIBILITY
------------------------
Login.aspx sets UnobtrusiveValidationMode="None" on the Page directive. This
allows the built-in Web Forms validators to work in a blank .NET Framework
project without requiring a jQuery ScriptResourceMapping.

The page code-behind also sets the HTTP response and charset to UTF-8. This
prevents symbols such as arrows from appearing as "â†’" in the browser.

CODEDOM CONFIGURATION FALLBACK
------------------------------
If the browser displays "The CodeDom provider ... could not be located", stop
IIS Express and remove the entire <system.codedom>...</system.codedom> block
from Web.config. That block requires a NuGet provider DLL that is missing from
the project. Then delete bin and obj, choose Build > Clean Solution, and use
Build > Rebuild Solution. The included C# files do not require that optional
provider block.

TEMPORARY DEMO ACCOUNTS
-----------------------
Learner
Email:    learner@codequest.io
Password: Learner123!

Administrator
Email:    admin@codequest.io
Password: Admin123!

IMPORTANT
---------
The demo passwords are hard-coded only to test the first page. Do not use this
approach in the completed system. The next authentication step should store
users in SQL Server and store password hashes, never plain-text passwords.

AdminDashboard.aspx is now a protected content overview for Admin accounts.
It reads course, module, chapter, tutorial, exercise and quiz counts from the
database and lists recent courses. Features/Admin/Content.aspx adds protected
course, module and chapter creation plus module publishing controls.
Features/Admin/Lessons.aspx adds public tutorial/exercise and learner quiz
authoring, including answer choices.

The Courses.aspx, Tutorials.aspx, About.aspx and ForgotPassword.aspx links are
prepared for future pages. Contact.aspx now routes to the authenticated support
ticket workspace.

Google login is currently a visual/testing button. A real Google sign-in needs
an OAuth client ID, client secret, callback URL and backend configuration.

Registration now inserts a learner into dbo.User using a salted PBKDF2 hash.
Login checks dbo.User first and keeps the two demo accounts as a temporary
fallback while the database is being configured.

The public course cards now open Features/Learner/Enroll.aspx with the selected
CourseID. Guests are sent to Login and returned to that course after signing in.
The enrolment step writes a Beginner or Premium-eligible course to
dbo.Enrollment, then redirects to LearnerDashboard.aspx.

The catalogue is session-aware: guests see "Log in to enrol", signed-in learners
see "Enrol now", and enrolled learners see "Continue course". The learner
dashboard Continue link opens Features/Learner/Course.aspx, which checks
Enrollment and displays published Module and Chapter content.

The demo seed now adds published HTML, CSS and JavaScript tutorial content,
matching Tutorial and Exercise rows, and HTML/CSS checkpoint Quiz questions.
Course chapter links open Features/Learner/Chapter.aspx and require a logged-in
learner with an Enrollment record. Public tutorials and exercises use
Features/Public/Tutorials.aspx and Features/Public/Tutorial.aspx and do not
require login. Login is reserved for learner-only course chapters, quizzes and
saving completion/progress records. Run Database/Progress_Extension.sql once
against CodeQuestDB before submitting a quiz or using tutorial category
filters; it adds ChapterProgress, QuizAttempt and Tutorial.category.

All future website images should be placed in Content/Images. Reference them
from an .aspx page with a path such as Content/Images/example.png.

If Visual Studio says a control is missing from the designer file, right-click
Login.aspx and choose "Convert to Web Application" to regenerate the designer.
