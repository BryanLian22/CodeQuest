/*
    CodeQuest learner progress extension.
    Run this after CodeQuest_Database.sql.

    These tables/columns extend the supplied ERD for persisted chapter
    completion, quiz attempts and public tutorial categories. The script is
    safe to run again.
*/

USE [CodeQuestDB];
GO

IF OBJECT_ID(N'dbo.ChapterProgress', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChapterProgress
    (
        ProgressID   INT IDENTITY(1,1) NOT NULL,
        UserID       INT               NOT NULL,
        ChapterID    INT               NOT NULL,
        status       NVARCHAR(20)      NOT NULL CONSTRAINT DF_ChapterProgress_Status DEFAULT (N'Completed'),
        completed_at DATETIME2(0)      NULL,

        CONSTRAINT PK_ChapterProgress PRIMARY KEY CLUSTERED (ProgressID),
        CONSTRAINT UQ_ChapterProgress_UserChapter UNIQUE (UserID, ChapterID),
        CONSTRAINT CK_ChapterProgress_Status CHECK (status IN (N'In Progress', N'Completed')),
        CONSTRAINT FK_ChapterProgress_User FOREIGN KEY (UserID)
            REFERENCES dbo.[User](UserID)
            ON DELETE CASCADE,
        CONSTRAINT FK_ChapterProgress_Chapter FOREIGN KEY (ChapterID)
            REFERENCES dbo.Chapter(ChapterID)
            ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'dbo.QuizAttempt', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.QuizAttempt
    (
        AttemptID      INT IDENTITY(1,1) NOT NULL,
        UserID         INT               NOT NULL,
        QuizID         INT               NOT NULL,
        ChapterID      INT               NOT NULL,
        selected_answer NVARCHAR(2000)   NULL,
        is_correct     BIT               NOT NULL,
        attempted_at   DATETIME2(0)      NOT NULL CONSTRAINT DF_QuizAttempt_AttemptedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_QuizAttempt PRIMARY KEY CLUSTERED (AttemptID),
        CONSTRAINT FK_QuizAttempt_User FOREIGN KEY (UserID)
            REFERENCES dbo.[User](UserID)
            ON DELETE CASCADE,
        CONSTRAINT FK_QuizAttempt_Quiz FOREIGN KEY (QuizID)
            REFERENCES dbo.Quiz(QuizID)
            ON DELETE CASCADE,
        CONSTRAINT FK_QuizAttempt_Chapter FOREIGN KEY (ChapterID)
            REFERENCES dbo.Chapter(ChapterID)
            ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ChapterProgress_UserID' AND object_id = OBJECT_ID(N'dbo.ChapterProgress'))
    CREATE INDEX IX_ChapterProgress_UserID ON dbo.ChapterProgress(UserID);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_QuizAttempt_UserID' AND object_id = OBJECT_ID(N'dbo.QuizAttempt'))
    CREATE INDEX IX_QuizAttempt_UserID ON dbo.QuizAttempt(UserID);
GO

/* Public tutorial categories used by the guest tutorial catalogue. */
IF COL_LENGTH(N'dbo.Tutorial', N'category') IS NULL
BEGIN
    ALTER TABLE dbo.Tutorial ADD category NVARCHAR(30) NULL;
END;
GO

/* This is a separate batch because SQL Server compiles a batch before the
   ALTER TABLE above executes. */
IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Tutorial')
      AND name = N'category'
      AND is_nullable = 1
)
BEGIN
    UPDATE dbo.Tutorial
    SET category = CASE
        WHEN tutorial_title LIKE N'%CSS%'
          OR tutorial_title LIKE N'%Box Model%'
          OR tutorial_title LIKE N'%Flexbox%'
          OR tutorial_title LIKE N'%Responsive%'
          OR tutorial_title LIKE N'%Selectors%' THEN N'CSS'
        WHEN tutorial_title LIKE N'%JavaScript%'
          OR tutorial_title LIKE N'%DOM%'
          OR tutorial_title LIKE N'%Functions%'
          OR tutorial_title LIKE N'%Variables%' THEN N'JavaScript'
        ELSE N'HTML'
    END;

    ALTER TABLE dbo.Tutorial ALTER COLUMN category NVARCHAR(30) NOT NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.default_constraints
    WHERE name = N'DF_Tutorial_Category'
      AND parent_object_id = OBJECT_ID(N'dbo.Tutorial')
)
    ALTER TABLE dbo.Tutorial ADD CONSTRAINT DF_Tutorial_Category DEFAULT (N'HTML') FOR category;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Tutorial_Category'
      AND object_id = OBJECT_ID(N'dbo.Tutorial')
)
    CREATE INDEX IX_Tutorial_Category ON dbo.Tutorial(category, status);
GO
