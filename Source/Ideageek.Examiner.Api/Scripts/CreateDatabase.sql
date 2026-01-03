IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'Examiner')
BEGIN
    PRINT 'Creating Examiner database';
    EXEC('CREATE DATABASE [Examiner]');
END
GO

USE [Examiner];
GO

-- School table
IF OBJECT_ID(N'dbo.School', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.School
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(200) NOT NULL,
        Code NVARCHAR(50) NOT NULL UNIQUE,
        Address NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
    );
END
GO

-- Class table
IF OBJECT_ID(N'dbo.Class', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Class
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        SchoolId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        Section NVARCHAR(50) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT FK_Class_School FOREIGN KEY (SchoolId) REFERENCES dbo.School(Id)
    );
END
GO

-- Student table
IF OBJECT_ID(N'dbo.Student', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Student
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        SchoolId UNIQUEIDENTIFIER NOT NULL,
        ClassId UNIQUEIDENTIFIER NOT NULL,
        StudentNumber NVARCHAR(50) NOT NULL UNIQUE,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT FK_Student_School FOREIGN KEY (SchoolId) REFERENCES dbo.School(Id),
        CONSTRAINT FK_Student_Class FOREIGN KEY (ClassId) REFERENCES dbo.Class(Id)
    );
END
GO

-- UserAccount table
IF OBJECT_ID(N'dbo.UserAccount', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserAccount
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        Username NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(500) NOT NULL,
        Role NVARCHAR(50) NOT NULL,
        StudentId UNIQUEIDENTIFIER NULL,
        TeacherId UNIQUEIDENTIFIER NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT FK_UserAccount_Student FOREIGN KEY (StudentId) REFERENCES dbo.Student(Id)
    );
END
GO

-- Exam table
IF OBJECT_ID(N'dbo.Exam', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Exam
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        SchoolId UNIQUEIDENTIFIER NOT NULL,
        ClassId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Subject NVARCHAR(200) NOT NULL,
        TotalMarks INT NOT NULL,
        QuestionCount INT NOT NULL,
        Type INT NOT NULL CONSTRAINT DF_Exam_Type DEFAULT(0),
        ExamDate DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        QuestionSheetFileName NVARCHAR(400) NULL,
        AnswerSheetFileName NVARCHAR(400) NULL,
        CONSTRAINT FK_Exam_School FOREIGN KEY (SchoolId) REFERENCES dbo.School(Id),
        CONSTRAINT FK_Exam_Class FOREIGN KEY (ClassId) REFERENCES dbo.Class(Id)
    );
END
GO

IF COL_LENGTH(N'dbo.Exam', N'QuestionSheetFileName') IS NULL
BEGIN
    ALTER TABLE dbo.Exam ADD QuestionSheetFileName NVARCHAR(400) NULL;
END
GO

IF COL_LENGTH(N'dbo.Exam', N'AnswerSheetFileName') IS NULL
BEGIN
    ALTER TABLE dbo.Exam ADD AnswerSheetFileName NVARCHAR(400) NULL;
END
GO

IF COL_LENGTH(N'dbo.Exam', N'Type') IS NULL
BEGIN
    ALTER TABLE dbo.Exam ADD Type INT NOT NULL CONSTRAINT DF_Exam_Type DEFAULT(0);
END
GO

-- Question table
IF OBJECT_ID(N'dbo.Question', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Question
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        ExamId UNIQUEIDENTIFIER NOT NULL,
        QuestionNumber INT NOT NULL,
        Text NVARCHAR(500) NOT NULL,
        CorrectOption CHAR(1) NOT NULL,
        Type INT NOT NULL CONSTRAINT DF_Question_Type DEFAULT(0),
        Lines INT NULL,
        Marks INT NULL,
        BoxSize INT NULL,
        CONSTRAINT FK_Question_Exam FOREIGN KEY (ExamId) REFERENCES dbo.Exam(Id)
    );
END
GO

IF COL_LENGTH(N'dbo.Question', N'OptionA') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Question DROP COLUMN OptionA;
END
GO

IF COL_LENGTH(N'dbo.Question', N'OptionB') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Question DROP COLUMN OptionB;
END
GO

IF COL_LENGTH(N'dbo.Question', N'OptionC') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Question DROP COLUMN OptionC;
END
GO

IF COL_LENGTH(N'dbo.Question', N'OptionD') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Question DROP COLUMN OptionD;
END
GO

IF COL_LENGTH(N'dbo.Question', N'Type') IS NULL
BEGIN
    ALTER TABLE dbo.Question ADD Type INT NOT NULL CONSTRAINT DF_Question_Type DEFAULT(0);
END
GO

IF COL_LENGTH(N'dbo.Question', N'Lines') IS NULL
BEGIN
    ALTER TABLE dbo.Question ADD Lines INT NULL;
END
GO

IF COL_LENGTH(N'dbo.Question', N'Marks') IS NULL
BEGIN
    ALTER TABLE dbo.Question ADD Marks INT NULL;
END
GO

IF COL_LENGTH(N'dbo.Question', N'BoxSize') IS NULL
BEGIN
    ALTER TABLE dbo.Question ADD BoxSize INT NULL;
END
GO

-- QuestionOption table
IF OBJECT_ID(N'dbo.QuestionOption', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.QuestionOption
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        QuestionId UNIQUEIDENTIFIER NOT NULL,
        [Key] NVARCHAR(10) NOT NULL,
        Text NVARCHAR(500) NOT NULL,
        [Order] INT NOT NULL,
        CONSTRAINT FK_QuestionOption_Question FOREIGN KEY (QuestionId) REFERENCES dbo.Question(Id)
    );
END
GO

-- QuestionSheetTemplate table
IF OBJECT_ID(N'dbo.QuestionSheetTemplate', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.QuestionSheetTemplate
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        ExamId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500) NULL,
        IsDefault BIT NOT NULL DEFAULT(1),
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT FK_QSheetTemplate_Exam FOREIGN KEY (ExamId) REFERENCES dbo.Exam(Id)
    );
END
GO

-- AnswerSheet table
IF OBJECT_ID(N'dbo.AnswerSheet', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AnswerSheet
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        ExamId UNIQUEIDENTIFIER NOT NULL,
        StudentId UNIQUEIDENTIFIER NOT NULL,
        StudentNumber NVARCHAR(50) NOT NULL,
        SheetCode NVARCHAR(100) NOT NULL,
        GeneratedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        ScannedAt DATETIME2 NULL,
        TotalMarks INT NULL,
        CorrectCount INT NULL,
        WrongCount INT NULL,
        BlankCount INT NULL,
        CONSTRAINT UQ_AnswerSheet_SheetCode UNIQUE (SheetCode),
        CONSTRAINT FK_AnswerSheet_Exam FOREIGN KEY (ExamId) REFERENCES dbo.Exam(Id),
        CONSTRAINT FK_AnswerSheet_Student FOREIGN KEY (StudentId) REFERENCES dbo.Student(Id)
    );
END
GO

-- AnswerSheetDetail table
IF OBJECT_ID(N'dbo.AnswerSheetDetail', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AnswerSheetDetail
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        AnswerSheetId UNIQUEIDENTIFIER NOT NULL,
        QuestionId UNIQUEIDENTIFIER NOT NULL,
        QuestionNumber INT NOT NULL,
        SelectedOption CHAR(1) NULL,
        IsCorrect BIT NULL,
        Marks INT NULL,
        CONSTRAINT FK_AnswerSheetDetail_AnswerSheet FOREIGN KEY (AnswerSheetId) REFERENCES dbo.AnswerSheet(Id),
        CONSTRAINT FK_AnswerSheetDetail_Question FOREIGN KEY (QuestionId) REFERENCES dbo.Question(Id)
    );
END
GO

-- Seed data
DECLARE @SchoolId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.School WHERE Code = N'DEMO-SCH');
IF @SchoolId IS NULL
BEGIN
    SET @SchoolId = NEWID();
    INSERT INTO dbo.School (Id, Name, Code, Address)
    VALUES (@SchoolId, N'Demo School', N'DEMO-SCH', N'Seeded address');
END

DECLARE @ClassGrade8 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.Class WHERE SchoolId = @SchoolId AND Name = N'Grade 8' AND ISNULL(Section,'') = N'A');
IF @ClassGrade8 IS NULL
BEGIN
    SET @ClassGrade8 = NEWID();
    INSERT INTO dbo.Class (Id, SchoolId, Name, Section)
    VALUES (@ClassGrade8, @SchoolId, N'Grade 8', N'A');
END

DECLARE @ClassGrade9 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.Class WHERE SchoolId = @SchoolId AND Name = N'Grade 9' AND ISNULL(Section,'') = N'A');
IF @ClassGrade9 IS NULL
BEGIN
    SET @ClassGrade9 = NEWID();
    INSERT INTO dbo.Class (Id, SchoolId, Name, Section)
    VALUES (@ClassGrade9, @SchoolId, N'Grade 9', N'A');
END

IF NOT EXISTS (SELECT 1 FROM dbo.Student WHERE StudentNumber = N'STD-0001')
BEGIN
    INSERT INTO dbo.Student (Id, SchoolId, ClassId, StudentNumber, FirstName, LastName)
    VALUES (NEWID(), @SchoolId, @ClassGrade8, N'STD-0001', N'Ali', N'Khan');
END

IF NOT EXISTS (SELECT 1 FROM dbo.Student WHERE StudentNumber = N'STD-0002')
BEGIN
    INSERT INTO dbo.Student (Id, SchoolId, ClassId, StudentNumber, FirstName, LastName)
    VALUES (NEWID(), @SchoolId, @ClassGrade9, N'STD-0002', N'Sara', N'Ahmed');
END

DECLARE @Student1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.Student WHERE StudentNumber = N'STD-0001');
DECLARE @Student2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.Student WHERE StudentNumber = N'STD-0002');

IF NOT EXISTS (SELECT 1 FROM dbo.UserAccount WHERE Username = N'superadmin@examiner.com')
BEGIN
    INSERT INTO dbo.UserAccount (Id, Username, PasswordHash, Role, CreatedAt)
    VALUES (NEWID(), N'superadmin@examiner.com', N'Zg09VMUr6OJJb9j6NlZHHQ==.LDxkywTIiyBIhBWl93Tgs69wrEASuEvLb8DnVKFo6cE=', N'SuperAdmin', SYSDATETIME());
END

IF @Student1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.UserAccount WHERE StudentId = @Student1)
BEGIN
    INSERT INTO dbo.UserAccount (Id, Username, PasswordHash, Role, StudentId, CreatedAt)
    VALUES (NEWID(), N'STD-0001', N'iOXrr7D45agLnt4/d4fYgQ==.gVmXS+PfTLwsWw0Wv3Vc13mIMyRb7pcOKxG/gCLutjg=', N'Student', @Student1, SYSDATETIME());
END

IF @Student2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.UserAccount WHERE StudentId = @Student2)
BEGIN
    INSERT INTO dbo.UserAccount (Id, Username, PasswordHash, Role, StudentId, CreatedAt)
    VALUES (NEWID(), N'STD-0002', N'nwFlr0fNpcZorZ7KJySf5g==.RXVTSYtIuafGpEiE+NN21gsxC31cYHacmKY1B6o7tTM=', N'Student', @Student2, SYSDATETIME());
END

DECLARE @ExamId UNIQUEIDENTIFIER = 'A9E7DC13-C9F7-44B0-9D13-148771AB0B1B';
IF NOT EXISTS (SELECT 1 FROM dbo.Exam WHERE Id = @ExamId)
BEGIN
    INSERT INTO dbo.Exam (Id, SchoolId, ClassId, Name, Subject, TotalMarks, QuestionCount, ExamDate, Type)
    VALUES (@ExamId, @SchoolId, @ClassGrade8, N'Demo MCQ Test', N'Mathematics', 10, 10, SYSDATETIME(), 0);
END

IF NOT EXISTS (SELECT 1 FROM dbo.QuestionSheetTemplate WHERE ExamId = @ExamId AND Name = N'Default OMR Template')
BEGIN
    INSERT INTO dbo.QuestionSheetTemplate (Id, ExamId, Name, Description, IsDefault)
    VALUES (NEWID(), @ExamId, N'Default OMR Template', N'Template used for demo 10-question sheet.', 1);
END
GO

-- Ensure demo diagram question uses half-page box
DECLARE @DiagramQuestionId UNIQUEIDENTIFIER = '0c1f7d9d-2fb2-4d7b-b5aa-6f5c1a8f7a11';
DECLARE @DetailedExamId UNIQUEIDENTIFIER = 'B4C1A8F7-52D2-4E71-9B5D-51F9C0B42F18';

IF NOT EXISTS (SELECT 1 FROM dbo.Exam WHERE Id = @DetailedExamId)
BEGIN
    INSERT INTO dbo.Exam (Id, SchoolId, ClassId, Name, Subject, TotalMarks, QuestionCount, ExamDate, Type)
    VALUES (@DetailedExamId, @SchoolId, @ClassGrade9, N'Demo Detailed Test', N'Science', 35, 4, SYSDATETIME(), 1);
END

IF EXISTS (SELECT 1 FROM dbo.Question WHERE Id = @DiagramQuestionId)
BEGIN
    UPDATE dbo.Question
    SET Type = 2, Marks = 30, Lines = NULL, BoxSize = 2
    WHERE Id = @DiagramQuestionId;
END
ELSE
BEGIN
    INSERT INTO dbo.Question (Id, ExamId, QuestionNumber, Text, CorrectOption, Type, Lines, Marks, BoxSize)
    VALUES (@DiagramQuestionId, @DetailedExamId, 4, N'Draw a labeled diagram of the human heart.', 'A', 2, NULL, 30, 2);
END
