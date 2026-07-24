CODEQUEST LOGIN PAGE - ASP.NET WEB FORMS (.NET FRAMEWORK 4.7.2)
================================================================

FILES
-----
Login.aspx                         Login page markup
Login.aspx.cs                      C# code-behind and temporary demo login
Login.aspx.designer.cs             Web Forms control declarations
GoogleCallback.aspx                Google OAuth callback page
GoogleCallback.aspx.cs             OAuth state validation and account linking
ForgotPassword.aspx                Password-reset request page
ForgotPassword.aspx.cs             Token issue and optional SMTP delivery
ResetPassword.aspx                 One-time password reset page
ResetPassword.aspx.cs              Token validation and transactional reset
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
Data/DatabaseInitializer.cs        Automatic schema and demo-data setup
Data/GoogleOAuthClient.cs          Server-side Google OpenID Connect client
Features/                          Feature folders and ERD mapping
Features/Public/Courses.aspx      First database-driven public page
Features/Public/Tutorials.aspx    Guest-accessible tutorial catalogue
Features/Learner/Enroll.aspx       Course-specific enrolment step
Features/Learner/Course.aspx       Enrolled course and published modules
Features/Learner/Chapter.aspx      Chapter tutorial and practice page
Features/Learner/Quiz.aspx         Enrolled learner checkpoint quiz
Features/Learner/Profile.aspx      Learner profile and secure password change
Features/AI/Assistant.aspx        Premium Google AI Studio learning assistant
Features/Billing/Plans.aspx        Basic/Premium plans and demo checkout
LearnerDashboard.aspx              Database-backed learner dashboard
AdminDashboard.aspx                Protected database-backed admin overview
Features/Admin/Content.aspx        Admin course, module and chapter authoring
Features/Admin/Lessons.aspx        Admin tutorial, exercise and quiz authoring
Features/Admin/Users.aspx          Admin user search and role/plan controls
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

Learners can update their username and biography from
Features/Learner/Profile.aspx. Password changes verify the current password and
save a new salted PBKDF2 hash. The email remains read-only in this version so a
profile edit cannot accidentally change the account's sign-in identity.

Features/Admin/Users.aspx searches dbo.User by username or email and shows safe
account details plus enrolment and support-ticket totals. Administrators can
change role and plan, but cannot remove their own Admin role or demote the final
administrator. Password hashes are never loaded into the admin-facing model.

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
   Remember to include Models/UserManagementRecord.cs,
   Features/Learner/Profile.aspx and Features/Admin/Users.aspx together with
   their code-behind/designer files and the two new CSS files.
8. Right-click Login.aspx and choose "Set as Start Page".
9. Press Ctrl+F5 to run the website.

DATABASE SETUP AFTER A FRESH CLONE
----------------------------------
Install SQL Server Express LocalDB (included with Visual Studio's Data storage
and processing workload), then run the website. On its first database request,
the shared connection factory automatically creates the configured database,
applies the main schema and progress extension, and inserts the demo content.
There is no query to paste into Visual Studio. Existing data is preserved when
the scripts run again. See Database/CodeQuest_Database_Setup.txt for config
switches, manual fallback steps and production guidance.

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
prevents arrows and other symbols from appearing as garbled text in the
browser.

SOURCE-CODE COMMENTS
--------------------
Every hand-maintained ASPX page, C# source file, stylesheet and JavaScript file
starts with a short purpose comment. Complex authentication and navigation
flows also include major-section comments. The SQL scripts retain their
introductory setup and safety notes. Visual Studio-generated *.designer.cs
files are intentionally left with their generated-code headers because manual
changes to those files are overwritten when the Web Forms designer runs.

CODEDOM CONFIGURATION FALLBACK
------------------------------
If the browser displays "The CodeDom provider ... could not be located", stop
IIS Express and remove the entire <system.codedom>...</system.codedom> block
from Web.config. That block requires a NuGet provider DLL that is missing from
the project. Then delete bin and obj, choose Build > Clean Solution, and use
Build > Rebuild Solution. The included C# files do not require that optional
provider block.

TEMPORARY DEMO ADMINISTRATOR
----------------------------
Email:    admin@codequest.io
Password: Admin123!

IMPORTANT
---------
The administrator demo password is hard-coded only as a marking and
demonstration fallback. Registered learners authenticate through SQL Server
using salted PBKDF2 password hashes; plain-text learner passwords are not
stored.

AdminDashboard.aspx is now a protected content overview for Admin accounts.
It reads course, module, chapter, tutorial, exercise and quiz counts from the
database and lists recent courses. Features/Admin/Content.aspx adds protected
course, module and chapter creation plus module publishing controls.
Features/Admin/Lessons.aspx adds public tutorial/exercise and learner quiz
authoring, including answer choices.

The Courses.aspx, Tutorials.aspx and About.aspx links remain part of the public
navigation. Contact.aspx now routes to the authenticated support ticket
workspace.

Google login now uses a server-side OAuth authorization-code callback at
GoogleCallback.aspx. It validates the state value, verifies the Google email,
links an existing dbo.User by email when safe, or creates a Basic learner with
a random unusable local password. Configure CodeQuestGoogleClientId,
CodeQuestGoogleClientSecret and CodeQuestGoogleRedirectUri in Web.config; the
redirect URI must exactly match the OAuth Web application configuration.

Registration now inserts a learner into dbo.User using a salted PBKDF2 hash.
Login checks dbo.User first and keeps only the demo administrator account as a
temporary fallback while the database is being configured.

ForgotPassword.aspx creates a 30-minute, one-time password-reset token in
dbo.Token. Only a SHA-256 digest is stored in the database. If SMTP app settings
are present, the link is emailed; otherwise the local prototype shows a
development-only link so the complete flow can be tested. ResetPassword.aspx
consumes the token and replaces the account password inside a SQL transaction.

The Premium AI assistant is available at Features/AI/Assistant.aspx. It checks
the current learner plan from dbo.User, sends course-aware prompts through the
server-side Google AI Studio Gemini client, and keeps temporary conversation
history in Session until a persistent conversation table is added. Keep
CodeQuestGoogleAiApiKey in Web.config only; never commit it to GitHub or expose
it in browser JavaScript.

The public course cards now open Features/Learner/Enroll.aspx with the selected
CourseID. Guests are sent to Login and returned to that course after signing in.
The enrolment step writes a Beginner or Premium-eligible course to
dbo.Enrollment, then redirects to LearnerDashboard.aspx.

The catalogue is session-aware: guests see "Log in to enrol", signed-in learners
see "Enrol now", and enrolled learners see "Continue course". The learner
dashboard Continue link opens Features/Learner/Course.aspx, which checks
Enrollment and displays published Module and Chapter content.

Authenticated learner navigation now uses Features/Learner/Courses.aspx rather
than the public guest catalogue. This learner-only catalogue keeps the Dashboard,
My learning, AI assistant, Profile, Support and Sign out header and displays
Enrol now, Continue course or Review course from the learner's enrollment status.

When an administrator opens the public course or tutorial catalogue, the page
now keeps the Admin workspace navigation and labels the actions as previews.
Administrators can open published courses, chapters, exercises and quizzes to
test them without an enrollment. Admin quiz attempts and chapter views are
read-only previews and do not write ChapterProgress or QuizAttempt records.
The public home header is also session-aware. Administrators returning through
View site see an Admin button back to their workspace, learners see Dashboard
and Sign out, and signed-out visitors continue to see Login and Get Started.
The public tutorial catalogue and tutorial detail pages use the same learner
account actions instead of displaying guest Login and Get Started links.

Shared authentication, public, learner and admin styles enable a short
same-origin page transition with a fade and vertical movement. Browsers that
do not support cross-document View Transitions use the page-entry animation,
while reduced-motion preferences disable the effect.

All pages with a CodeQuest header load the shared responsive navigation
script. On tablet and phone widths it creates an accessible menu button,
places the current account actions inside the dropdown, closes after a
selection or Escape, and keeps long admin navigation lists scrollable.
Admin and learner dashboard grids, headings, badges and course cards also
collapse safely at narrow widths.
Published public tutorial exercises can also be tested while retaining the
Admin header.

The demo seed now adds published HTML, CSS and JavaScript tutorial content,
matching Tutorial and Exercise rows, and HTML/CSS checkpoint Quiz questions.
Course chapter links open Features/Learner/Chapter.aspx and require a logged-in
learner with an Enrollment record. Public tutorials and exercises use
Features/Public/Tutorials.aspx and Features/Public/Tutorial.aspx and do not
require login. Login is reserved for learner-only course chapters, quizzes and
saving completion/progress records. Automatic database initialization adds
ChapterProgress, QuizAttempt and Tutorial.category before these features first
open.

Opening an enrolled learner chapter marks its ChapterProgress as Completed, and
the course directory displays a Done badge beside viewed chapters. When every
chapter in the course's published modules has been viewed,
dbo.Enrollment.status changes to Completed and the learner dashboard shows
Review course. Chapter and quiz breadcrumbs link back to the learner dashboard,
course and chapter pages. The dashboard also recalculates enrollment completion
when it loads. Returning to a quiz restores the learner's latest saved
selections and latest score. Quiz attempts require at least 75 percent to pass.
A failed attempt offers Retake quiz only; a passed attempt offers both Retake
quiz and the next published chapter (or returns to the completed course when it
was the final chapter). The chapter lesson itself also provides next-chapter
navigation.

All future website images should be placed in Content/Images. Reference them
from an .aspx page with a path such as Content/Images/example.png.

If Visual Studio says a control is missing from the designer file, right-click
Login.aspx and choose "Convert to Web Application" to regenerate the designer.
