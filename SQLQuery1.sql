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
END;
GO

SELECT CourseID, course_title, difficulty
FROM dbo.Course
ORDER BY CourseID;
GO
