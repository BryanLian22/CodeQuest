/*
    Optional CodeQuest demo data.
    Run this after CodeQuest_Database.sql.

    The admin password below is intentionally not a usable password hash.
    The current prototype login still supports the demo admin account while
    a real admin account is created through a secure administration workflow.
*/

USE [CodeQuestDB];
GO

IF NOT EXISTS (SELECT 1 FROM dbo.[User] WHERE email = N'admin@codequest.io')
BEGIN
    INSERT INTO dbo.[User](username, [password], email, bio, role, [plan])
    VALUES
    (
        N'admin',
        N'DEMO_ONLY_REPLACE_WITH_PBKDF2_HASH',
        N'admin@codequest.io',
        N'CodeQuest platform administrator',
        N'Admin',
        N'Premium'
    );
END;
GO

DECLARE @AdminID INT =
(
    SELECT TOP (1) UserID
    FROM dbo.[User]
    WHERE email = N'admin@codequest.io'
);

IF @AdminID IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.Course WHERE course_title = N'HTML Foundations')
        INSERT INTO dbo.Course(UserID, course_title, description, difficulty)
        VALUES (@AdminID, N'HTML Foundations', N'Build the backbone of every webpage with semantic structure, forms, media and accessibility.', N'Beginner');

    IF NOT EXISTS (SELECT 1 FROM dbo.Course WHERE course_title = N'CSS Essentials')
        INSERT INTO dbo.Course(UserID, course_title, description, difficulty)
        VALUES (@AdminID, N'CSS Essentials', N'Style, lay out and create responsive interfaces with modern CSS fundamentals.', N'Beginner');

    IF NOT EXISTS (SELECT 1 FROM dbo.Course WHERE course_title = N'JavaScript Basics')
        INSERT INTO dbo.Course(UserID, course_title, description, difficulty)
        VALUES (@AdminID, N'JavaScript Basics', N'Add interactivity and logic to your websites with JavaScript essentials.', N'Intermediate');

    DECLARE @HtmlCourseID INT = (SELECT CourseID FROM dbo.Course WHERE course_title = N'HTML Foundations');

    IF NOT EXISTS (SELECT 1 FROM dbo.Module WHERE CourseID = @HtmlCourseID AND module_title = N'Introduction to HTML')
    BEGIN
        DECLARE @HtmlModuleID INT;
        INSERT INTO dbo.Module(CourseID, module_title, description, status)
        VALUES (@HtmlCourseID, N'Introduction to HTML', N'Learn how documents, elements and attributes form a webpage.', N'Published');
        SET @HtmlModuleID = SCOPE_IDENTITY();

        INSERT INTO dbo.Chapter(ModuleID, title, description)
        VALUES (@HtmlModuleID, N'How the Web Works', N'Understand the browser, server and document request cycle.'),
               (@HtmlModuleID, N'HTML Document Structure', N'Create the basic HTML document shell.'),
               (@HtmlModuleID, N'Tags and Elements', N'Use elements to give webpage content meaning.'),
               (@HtmlModuleID, N'Module Checkpoint', N'Check your understanding of HTML foundations.');
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Tutorial WHERE tutorial_title = N'HTML Document Structure')
    BEGIN
        DECLARE @TutorialID INT;
        INSERT INTO dbo.Tutorial(tutorial_title, materials, status)
        VALUES (N'HTML Document Structure', N'Article and code example: <!DOCTYPE html> with html, head and body elements.', N'Published');
        SET @TutorialID = SCOPE_IDENTITY();

        INSERT INTO dbo.Exercise(TutorialID, correct_answer, question)
        VALUES (@TutorialID, N'<!DOCTYPE html>', N'Which declaration tells the browser this is an HTML5 document?');
    END;

    DECLARE @CheckpointChapterID INT =
    (
        SELECT TOP (1) c.ChapterID
        FROM dbo.Chapter c
        INNER JOIN dbo.Module m ON m.ModuleID = c.ModuleID
        WHERE m.CourseID = @HtmlCourseID
          AND c.title = N'Module Checkpoint'
    );

    IF @CheckpointChapterID IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM dbo.Quiz WHERE ChapterID = @CheckpointChapterID)
    BEGIN
        DECLARE @QuizID INT;
        INSERT INTO dbo.Quiz(ChapterID, description, question, correct_answer)
        VALUES (@CheckpointChapterID, N'HTML foundations checkpoint', N'Which HTML attribute provides alternative text for an image?', N'alt');
        SET @QuizID = SCOPE_IDENTITY();

        INSERT INTO dbo.QuizAns(QuizID, Answer)
        VALUES (@QuizID, N'href'), (@QuizID, N'alt'), (@QuizID, N'src'), (@QuizID, N'title');
    END;

    /* CSS Essentials content for the learner workspace. */
    DECLARE @CssCourseID INT =
    (
        SELECT TOP (1) CourseID
        FROM dbo.Course
        WHERE course_title = N'CSS Essentials'
    );

    IF @CssCourseID IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.Module WHERE CourseID = @CssCourseID AND module_title = N'CSS Fundamentals')
        BEGIN
            INSERT INTO dbo.Module(CourseID, module_title, description, status)
            VALUES (@CssCourseID, N'CSS Fundamentals', N'Build a strong styling foundation with selectors, the box model, layout and responsive design.', N'Published');
        END;

        DECLARE @CssModuleID INT =
        (
            SELECT TOP (1) ModuleID
            FROM dbo.Module
            WHERE CourseID = @CssCourseID AND module_title = N'CSS Fundamentals'
        );

        IF NOT EXISTS (SELECT 1 FROM dbo.Chapter WHERE ModuleID = @CssModuleID AND title = N'Selectors and Specificity')
            INSERT INTO dbo.Chapter(ModuleID, title, description)
            VALUES (@CssModuleID, N'Selectors and Specificity', N'Target the right elements and understand which CSS rule wins.');

        IF NOT EXISTS (SELECT 1 FROM dbo.Chapter WHERE ModuleID = @CssModuleID AND title = N'The Box Model')
            INSERT INTO dbo.Chapter(ModuleID, title, description)
            VALUES (@CssModuleID, N'The Box Model', N'Control content, padding, borders and margins to create clean layouts.');

        IF NOT EXISTS (SELECT 1 FROM dbo.Chapter WHERE ModuleID = @CssModuleID AND title = N'Flexbox Layout')
            INSERT INTO dbo.Chapter(ModuleID, title, description)
            VALUES (@CssModuleID, N'Flexbox Layout', N'Arrange items along a row or column with the flexible box layout system.');

        IF NOT EXISTS (SELECT 1 FROM dbo.Chapter WHERE ModuleID = @CssModuleID AND title = N'Responsive Layouts')
            INSERT INTO dbo.Chapter(ModuleID, title, description)
            VALUES (@CssModuleID, N'Responsive Layouts', N'Adapt interfaces to different screens with fluid sizing and media queries.');

        IF NOT EXISTS (SELECT 1 FROM dbo.Tutorial WHERE tutorial_title = N'Selectors and Specificity')
        BEGIN
            DECLARE @SelectorsTutorialID INT;
            INSERT INTO dbo.Tutorial(tutorial_title, materials, status)
            VALUES
            (
                N'Selectors and Specificity',
                N'CSS selects HTML elements and applies declarations inside a rule. Example:\n\np {\n  color: #18324d;\n}\n\nUse a class selector such as .card when a style should be reusable.',
                N'Published'
            );
            SET @SelectorsTutorialID = SCOPE_IDENTITY();

            INSERT INTO dbo.Exercise(TutorialID, correct_answer, question)
            VALUES (@SelectorsTutorialID, N'p', N'Which selector targets every paragraph element?');
        END;

        IF NOT EXISTS (SELECT 1 FROM dbo.Tutorial WHERE tutorial_title = N'The Box Model')
        BEGIN
            DECLARE @BoxTutorialID INT;
            INSERT INTO dbo.Tutorial(tutorial_title, materials, status)
            VALUES
            (
                N'The Box Model',
                N'Every element is laid out as content, padding, border and margin. Example:\n\n.card {\n  padding: 1rem;\n  border: 1px solid #b9cce0;\n  margin: 0.75rem;\n}',
                N'Published'
            );
            SET @BoxTutorialID = SCOPE_IDENTITY();

            INSERT INTO dbo.Exercise(TutorialID, correct_answer, question)
            VALUES (@BoxTutorialID, N'padding', N'Which property controls space inside an element, between its content and border?');
        END;

        IF NOT EXISTS (SELECT 1 FROM dbo.Tutorial WHERE tutorial_title = N'Flexbox Layout')
        BEGIN
            DECLARE @FlexTutorialID INT;
            INSERT INTO dbo.Tutorial(tutorial_title, materials, status)
            VALUES
            (
                N'Flexbox Layout',
                N'Flexbox makes one-dimensional layouts easier to align. Example:\n\n.toolbar {\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n}',
                N'Published'
            );
            SET @FlexTutorialID = SCOPE_IDENTITY();

            INSERT INTO dbo.Exercise(TutorialID, correct_answer, question)
            VALUES (@FlexTutorialID, N'display: flex', N'Which declaration turns an element into a flex container?');
        END;

        IF NOT EXISTS (SELECT 1 FROM dbo.Tutorial WHERE tutorial_title = N'Responsive Layouts')
        BEGIN
            DECLARE @ResponsiveTutorialID INT;
            INSERT INTO dbo.Tutorial(tutorial_title, materials, status)
            VALUES
            (
                N'Responsive Layouts',
                N'Media queries apply styles when a condition is true. Example:\n\n@media (max-width: 700px) {\n  .course-grid {\n    grid-template-columns: 1fr;\n  }\n}',
                N'Published'
            );
            SET @ResponsiveTutorialID = SCOPE_IDENTITY();

            INSERT INTO dbo.Exercise(TutorialID, correct_answer, question)
            VALUES (@ResponsiveTutorialID, N'@media', N'Which CSS feature applies styles based on the viewport width?');
        END;

        DECLARE @SelectorsChapterID INT =
        (
            SELECT TOP (1) c.ChapterID
            FROM dbo.Chapter c
            WHERE c.ModuleID = @CssModuleID AND c.title = N'Selectors and Specificity'
        );

        IF @SelectorsChapterID IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM dbo.Quiz WHERE ChapterID = @SelectorsChapterID)
        BEGIN
            DECLARE @SelectorsQuizID INT;
            INSERT INTO dbo.Quiz(ChapterID, description, question, correct_answer)
            VALUES (@SelectorsChapterID, N'CSS selectors checkpoint', N'Which selector targets every paragraph element?', N'p');
            SET @SelectorsQuizID = SCOPE_IDENTITY();

            INSERT INTO dbo.QuizAns(QuizID, Answer)
            VALUES (@SelectorsQuizID, N'p'), (@SelectorsQuizID, N'.p'), (@SelectorsQuizID, N'#p'), (@SelectorsQuizID, N'* p');
        END;

        DECLARE @ResponsiveChapterID INT =
        (
            SELECT TOP (1) c.ChapterID
            FROM dbo.Chapter c
            WHERE c.ModuleID = @CssModuleID AND c.title = N'Responsive Layouts'
        );

        IF @ResponsiveChapterID IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM dbo.Quiz WHERE ChapterID = @ResponsiveChapterID)
        BEGIN
            DECLARE @CssQuizID INT;
            INSERT INTO dbo.Quiz(ChapterID, description, question, correct_answer)
            VALUES (@ResponsiveChapterID, N'Responsive CSS checkpoint', N'Which CSS feature applies styles based on the viewport width?', N'@media');
            SET @CssQuizID = SCOPE_IDENTITY();

            INSERT INTO dbo.QuizAns(QuizID, Answer)
            VALUES (@CssQuizID, N'@media'), (@CssQuizID, N'@font-face'), (@CssQuizID, N'@supports'), (@CssQuizID, N'@layer');
        END;
    END;

    /* Public JavaScript tutorials. The course remains Premium-gated, but
       these introductory tutorials are free to browse and practise. */
    DECLARE @JsCourseID INT =
    (
        SELECT TOP (1) CourseID
        FROM dbo.Course
        WHERE course_title = N'JavaScript Basics'
    );

    IF @JsCourseID IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.Module WHERE CourseID = @JsCourseID AND module_title = N'JavaScript Fundamentals')
        BEGIN
            INSERT INTO dbo.Module(CourseID, module_title, description, status)
            VALUES (@JsCourseID, N'JavaScript Fundamentals', N'Learn the core ideas behind interactive webpages with beginner-friendly JavaScript.', N'Published');
        END;

        DECLARE @JsModuleID INT =
        (
            SELECT TOP (1) ModuleID
            FROM dbo.Module
            WHERE CourseID = @JsCourseID AND module_title = N'JavaScript Fundamentals'
        );

        IF NOT EXISTS (SELECT 1 FROM dbo.Chapter WHERE ModuleID = @JsModuleID AND title = N'Variables and Values')
            INSERT INTO dbo.Chapter(ModuleID, title, description)
            VALUES (@JsModuleID, N'Variables and Values', N'Store text, numbers and other values with clear variable declarations.');

        IF NOT EXISTS (SELECT 1 FROM dbo.Chapter WHERE ModuleID = @JsModuleID AND title = N'Functions')
            INSERT INTO dbo.Chapter(ModuleID, title, description)
            VALUES (@JsModuleID, N'Functions', N'Group reusable instructions into functions and call them when needed.');

        IF NOT EXISTS (SELECT 1 FROM dbo.Chapter WHERE ModuleID = @JsModuleID AND title = N'DOM Events')
            INSERT INTO dbo.Chapter(ModuleID, title, description)
            VALUES (@JsModuleID, N'DOM Events', N'Respond to clicks and other browser events to make pages interactive.');

        IF NOT EXISTS (SELECT 1 FROM dbo.Tutorial WHERE tutorial_title = N'Variables and Values')
        BEGIN
            DECLARE @VariablesTutorialID INT;
            INSERT INTO dbo.Tutorial(tutorial_title, materials, status)
            VALUES (N'Variables and Values', N'JavaScript variables hold values that your program can use. Example:\n\nconst language = "JavaScript";\nlet lessons = 3;\n\nUse const when the binding should not be reassigned.', N'Published');
            SET @VariablesTutorialID = SCOPE_IDENTITY();
            INSERT INTO dbo.Exercise(TutorialID, correct_answer, question)
            VALUES (@VariablesTutorialID, N'const', N'Which keyword declares a value that should not be reassigned?');
        END;

        IF NOT EXISTS (SELECT 1 FROM dbo.Tutorial WHERE tutorial_title = N'Functions')
        BEGIN
            DECLARE @FunctionsTutorialID INT;
            INSERT INTO dbo.Tutorial(tutorial_title, materials, status)
            VALUES (N'Functions', N'Functions package reusable instructions. Example:\n\nfunction greet(name) {\n  return "Hello " + name;\n}', N'Published');
            SET @FunctionsTutorialID = SCOPE_IDENTITY();
            INSERT INTO dbo.Exercise(TutorialID, correct_answer, question)
            VALUES (@FunctionsTutorialID, N'function', N'Which keyword starts a traditional JavaScript function declaration?');
        END;

        IF NOT EXISTS (SELECT 1 FROM dbo.Tutorial WHERE tutorial_title = N'DOM Events')
        BEGIN
            DECLARE @DomEventsTutorialID INT;
            INSERT INTO dbo.Tutorial(tutorial_title, materials, status)
            VALUES (N'DOM Events', N'Event listeners run code when an interaction occurs. Example:\n\nbutton.addEventListener("click", function () {\n  console.log("Clicked");\n});', N'Published');
            SET @DomEventsTutorialID = SCOPE_IDENTITY();
            INSERT INTO dbo.Exercise(TutorialID, correct_answer, question)
            VALUES (@DomEventsTutorialID, N'addEventListener', N'Which method attaches a handler for a click event?');
        END;
    END;
END;
GO

SELECT CourseID, course_title, difficulty
FROM dbo.Course
ORDER BY CourseID;
GO
