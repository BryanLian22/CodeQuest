/*
    CodeQuest database schema
    Source: ERDCODEQUEST.drawio
    Target: Microsoft SQL Server / LocalDB

    This script follows the entities and relationships in the supplied ERD.
    Password values in [User].[password] must be salted password hashes from
    the application. Never store a plain-text password in this column.
*/

IF DB_ID(N'CodeQuestDB') IS NULL
BEGIN
    CREATE DATABASE [CodeQuestDB];
END;
GO

USE [CodeQuestDB];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

/* ================================================================
   1. USER AND ACCESS TABLES
   ================================================================ */

IF OBJECT_ID(N'dbo.[User]', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.[User]
    (
        UserID       INT IDENTITY(1,1) NOT NULL,
        username     NVARCHAR(50)      NOT NULL,
        [password]   NVARCHAR(255)     NOT NULL,
        email        NVARCHAR(254)     NOT NULL,
        bio          NVARCHAR(1000)    NULL,
        role         NVARCHAR(20)      NOT NULL CONSTRAINT DF_User_Role DEFAULT (N'Learner'),
        [plan]       NVARCHAR(20)      NOT NULL CONSTRAINT DF_User_Plan DEFAULT (N'Basic'),
        google_id    NVARCHAR(255)     NULL,

        CONSTRAINT PK_User PRIMARY KEY CLUSTERED (UserID),
        CONSTRAINT UQ_User_Username UNIQUE (username),
        CONSTRAINT UQ_User_Email UNIQUE (email),
        CONSTRAINT CK_User_Role CHECK (role IN (N'Learner', N'Admin')),
        CONSTRAINT CK_User_Plan CHECK ([plan] IN (N'Basic', N'Premium'))
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_User_GoogleID'
      AND object_id = OBJECT_ID(N'dbo.[User]')
)
BEGIN
    CREATE UNIQUE INDEX UX_User_GoogleID
        ON dbo.[User](google_id)
        WHERE google_id IS NOT NULL;
END;
GO

IF OBJECT_ID(N'dbo.Token', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Token
    (
        TokenID      INT IDENTITY(1,1) NOT NULL,
        UserID       INT               NOT NULL,
        token_type   NVARCHAR(30)      NOT NULL,
        token        NVARCHAR(255)     NOT NULL,
        expires_at   DATETIME2(0)      NOT NULL,
        used         BIT               NOT NULL CONSTRAINT DF_Token_Used DEFAULT (0),

        CONSTRAINT PK_Token PRIMARY KEY CLUSTERED (TokenID),
        CONSTRAINT UQ_Token_Token UNIQUE (token),
        CONSTRAINT FK_Token_User FOREIGN KEY (UserID)
            REFERENCES dbo.[User](UserID)
            ON DELETE CASCADE
    );
END;
GO

/* ================================================================
   2. COURSE CONTENT TABLES
   ================================================================ */

IF OBJECT_ID(N'dbo.Course', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Course
    (
        CourseID      INT IDENTITY(1,1) NOT NULL,
        UserID        INT               NOT NULL,
        course_title  NVARCHAR(150)     NOT NULL,
        description   NVARCHAR(MAX)    NULL,
        difficulty    NVARCHAR(20)      NOT NULL,

        CONSTRAINT PK_Course PRIMARY KEY CLUSTERED (CourseID),
        CONSTRAINT UQ_Course_Title UNIQUE (course_title),
        CONSTRAINT CK_Course_Difficulty CHECK (difficulty IN (N'Beginner', N'Intermediate', N'Advanced')),
        CONSTRAINT FK_Course_User FOREIGN KEY (UserID)
            REFERENCES dbo.[User](UserID)
    );
END;
GO

IF OBJECT_ID(N'dbo.Module', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Module
    (
        ModuleID      INT IDENTITY(1,1) NOT NULL,
        CourseID      INT               NOT NULL,
        module_title  NVARCHAR(150)     NOT NULL,
        description   NVARCHAR(MAX)    NULL,
        status        NVARCHAR(20)      NOT NULL CONSTRAINT DF_Module_Status DEFAULT (N'Draft'),

        CONSTRAINT PK_Module PRIMARY KEY CLUSTERED (ModuleID),
        CONSTRAINT CK_Module_Status CHECK (status IN (N'Draft', N'Published', N'Archived')),
        CONSTRAINT FK_Module_Course FOREIGN KEY (CourseID)
            REFERENCES dbo.Course(CourseID)
            ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'dbo.Chapter', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Chapter
    (
        ChapterID   INT IDENTITY(1,1) NOT NULL,
        ModuleID    INT               NOT NULL,
        title       NVARCHAR(150)     NOT NULL,
        description NVARCHAR(MAX)    NULL,

        CONSTRAINT PK_Chapter PRIMARY KEY CLUSTERED (ChapterID),
        CONSTRAINT FK_Chapter_Module FOREIGN KEY (ModuleID)
            REFERENCES dbo.Module(ModuleID)
            ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'dbo.Tutorial', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tutorial
    (
        TutorialID     INT IDENTITY(1,1) NOT NULL,
        tutorial_title NVARCHAR(200)     NOT NULL,
        materials      NVARCHAR(MAX)    NULL,
        status         NVARCHAR(20)      NOT NULL CONSTRAINT DF_Tutorial_Status DEFAULT (N'Draft'),

        CONSTRAINT PK_Tutorial PRIMARY KEY CLUSTERED (TutorialID),
        CONSTRAINT CK_Tutorial_Status CHECK (status IN (N'Draft', N'Published', N'Review'))
    );
END;
GO

IF OBJECT_ID(N'dbo.Exercise', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Exercise
    (
        ExerciseID     INT IDENTITY(1,1) NOT NULL,
        TutorialID     INT               NOT NULL,
        correct_answer NVARCHAR(2000)    NOT NULL,
        question       NVARCHAR(MAX)    NOT NULL,

        CONSTRAINT PK_Exercise PRIMARY KEY CLUSTERED (ExerciseID),
        CONSTRAINT FK_Exercise_Tutorial FOREIGN KEY (TutorialID)
            REFERENCES dbo.Tutorial(TutorialID)
            ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'dbo.Quiz', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Quiz
    (
        QuizID         INT IDENTITY(1,1) NOT NULL,
        ChapterID      INT               NOT NULL,
        description    NVARCHAR(MAX)    NULL,
        question       NVARCHAR(MAX)    NOT NULL,
        correct_answer NVARCHAR(2000)    NOT NULL,

        CONSTRAINT PK_Quiz PRIMARY KEY CLUSTERED (QuizID),
        CONSTRAINT FK_Quiz_Chapter FOREIGN KEY (ChapterID)
            REFERENCES dbo.Chapter(ChapterID)
            ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'dbo.QuizAns', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.QuizAns
    (
        QAnsID INT IDENTITY(1,1) NOT NULL,
        QuizID INT               NOT NULL,
        Answer NVARCHAR(2000)    NOT NULL,

        CONSTRAINT PK_QuizAns PRIMARY KEY CLUSTERED (QAnsID),
        CONSTRAINT FK_QuizAns_Quiz FOREIGN KEY (QuizID)
            REFERENCES dbo.Quiz(QuizID)
            ON DELETE CASCADE
    );
END;
GO

/* ================================================================
   3. LEARNER ENROLMENT AND BILLING TABLES
   ================================================================ */

IF OBJECT_ID(N'dbo.Enrollment', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Enrollment
    (
        EID      INT IDENTITY(1,1) NOT NULL,
        UserID   INT               NOT NULL,
        CourseID INT               NOT NULL,
        status   NVARCHAR(20)      NOT NULL CONSTRAINT DF_Enrollment_Status DEFAULT (N'Active'),

        CONSTRAINT PK_Enrollment PRIMARY KEY CLUSTERED (EID),
        CONSTRAINT UQ_Enrollment_UserCourse UNIQUE (UserID, CourseID),
        CONSTRAINT CK_Enrollment_Status CHECK (status IN (N'Pending', N'Active', N'Completed', N'Cancelled')),
        CONSTRAINT FK_Enrollment_User FOREIGN KEY (UserID)
            REFERENCES dbo.[User](UserID),
        CONSTRAINT FK_Enrollment_Course FOREIGN KEY (CourseID)
            REFERENCES dbo.Course(CourseID)
    );
END;
GO

IF OBJECT_ID(N'dbo.Subscription', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Subscription
    (
        SubscriptionID INT IDENTITY(1,1) NOT NULL,
        UserID         INT               NOT NULL,
        plan_type      NVARCHAR(20)      NOT NULL,
        billing_cycle  NVARCHAR(20)      NOT NULL,
        start_date     DATE              NOT NULL,
        end_date       DATE              NULL,
        status         NVARCHAR(20)      NOT NULL CONSTRAINT DF_Subscription_Status DEFAULT (N'Active'),

        CONSTRAINT PK_Subscription PRIMARY KEY CLUSTERED (SubscriptionID),
        CONSTRAINT CK_Subscription_Plan CHECK (plan_type IN (N'Basic', N'Premium')),
        CONSTRAINT CK_Subscription_Billing CHECK (billing_cycle IN (N'Monthly', N'Yearly', N'Lifetime')),
        CONSTRAINT CK_Subscription_Status CHECK (status IN (N'Pending', N'Active', N'Expired', N'Cancelled')),
        CONSTRAINT CK_Subscription_Dates CHECK (end_date IS NULL OR end_date >= start_date),
        CONSTRAINT FK_Subscription_User FOREIGN KEY (UserID)
            REFERENCES dbo.[User](UserID)
    );
END;
GO

IF OBJECT_ID(N'dbo.Payment', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payment
    (
        PaymentID       INT IDENTITY(1,1) NOT NULL,
        UserID          INT               NOT NULL,
        SubscriptionID  INT               NULL,
        amount          DECIMAL(10,2)     NOT NULL,
        transaction_ref NVARCHAR(100)     NULL,
        status          NVARCHAR(20)      NOT NULL CONSTRAINT DF_Payment_Status DEFAULT (N'Pending'),
        paid_at         DATETIME2(0)      NULL,

        CONSTRAINT PK_Payment PRIMARY KEY CLUSTERED (PaymentID),
        CONSTRAINT CK_Payment_Amount CHECK (amount >= 0),
        CONSTRAINT CK_Payment_Status CHECK (status IN (N'Pending', N'Completed', N'Failed', N'Refunded')),
        CONSTRAINT FK_Payment_User FOREIGN KEY (UserID)
            REFERENCES dbo.[User](UserID),
        CONSTRAINT FK_Payment_Subscription FOREIGN KEY (SubscriptionID)
            REFERENCES dbo.Subscription(SubscriptionID)
            ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_Payment_TransactionRef'
      AND object_id = OBJECT_ID(N'dbo.Payment')
)
BEGIN
    CREATE UNIQUE INDEX UX_Payment_TransactionRef
        ON dbo.Payment(transaction_ref)
        WHERE transaction_ref IS NOT NULL;
END;
GO

/* ================================================================
   4. CONTACT SUPPORT TABLES
   ================================================================ */

IF OBJECT_ID(N'dbo.Ticket', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Ticket
    (
        TicketID    INT IDENTITY(1,1) NOT NULL,
        UserID      INT               NOT NULL,
        name        NVARCHAR(100)     NOT NULL,
        email       NVARCHAR(254)     NOT NULL,
        category    NVARCHAR(40)      NOT NULL,
        subject     NVARCHAR(200)     NOT NULL,
        description NVARCHAR(MAX)    NOT NULL,
        status      NVARCHAR(20)      NOT NULL CONSTRAINT DF_Ticket_Status DEFAULT (N'Open'),

        CONSTRAINT PK_Ticket PRIMARY KEY CLUSTERED (TicketID),
        CONSTRAINT CK_Ticket_Status CHECK (status IN (N'Open', N'In Progress', N'Resolved', N'Closed')),
        CONSTRAINT FK_Ticket_User FOREIGN KEY (UserID)
            REFERENCES dbo.[User](UserID)
    );
END;
GO

IF OBJECT_ID(N'dbo.Reply', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reply
    (
        ReplyID   INT IDENTITY(1,1) NOT NULL,
        TicketID  INT               NOT NULL,
        UserID    INT               NOT NULL,
        message   NVARCHAR(MAX)    NOT NULL,
        created_at DATETIME2(0)    NOT NULL CONSTRAINT DF_Reply_CreatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_Reply PRIMARY KEY CLUSTERED (ReplyID),
        CONSTRAINT FK_Reply_Ticket FOREIGN KEY (TicketID)
            REFERENCES dbo.Ticket(TicketID)
            ON DELETE CASCADE,
        CONSTRAINT FK_Reply_User FOREIGN KEY (UserID)
            REFERENCES dbo.[User](UserID)
    );
END;
GO

/* ================================================================
   5. FOREIGN-KEY SUPPORTING INDEXES
   ================================================================ */

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Module_CourseID' AND object_id = OBJECT_ID(N'dbo.Module'))
    CREATE INDEX IX_Module_CourseID ON dbo.Module(CourseID);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Chapter_ModuleID' AND object_id = OBJECT_ID(N'dbo.Chapter'))
    CREATE INDEX IX_Chapter_ModuleID ON dbo.Chapter(ModuleID);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Enrollment_CourseID' AND object_id = OBJECT_ID(N'dbo.Enrollment'))
    CREATE INDEX IX_Enrollment_CourseID ON dbo.Enrollment(CourseID);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ticket_UserID' AND object_id = OBJECT_ID(N'dbo.Ticket'))
    CREATE INDEX IX_Ticket_UserID ON dbo.Ticket(UserID);
GO

/* ================================================================
   6. QUICK VERIFICATION
   ================================================================ */

SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
  AND TABLE_SCHEMA = 'dbo'
ORDER BY TABLE_NAME;
GO

/*
    ERD items to decide before the payment module is implemented:

    1. Payment currently points to Subscription, but the prototype also shows
       a one-course payment. Add CourseID or EnrollmentID to Payment if the
       system will sell individual courses separately from subscriptions.
    2. The ERD does not contain lesson-progress or quiz-attempt tables. Add
       those tables before building persistent learner progress and quiz history.
*/
