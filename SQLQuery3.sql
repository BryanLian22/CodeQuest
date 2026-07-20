/*
    CodeQuest learner progress extension.
    Run this after CodeQuest_Database.sql.

    These tables extend the supplied ERD only for persisted chapter completion
    and quiz attempts. The script is safe to run again.
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
