-- ====================================================
-- SQL Server 2017 Database Schema
-- Project: Student Equivalent Certificate Registry
-- Compatible with: Windows Server 2022 / SQL Server 2017
-- ====================================================

-- 1. Create Database if not exists
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'StudentRegistryDb')
BEGIN
    CREATE DATABASE [StudentRegistryDb];
END
GO

USE [StudentRegistryDb];
GO

-- 2. Drop existing tables if they exist (in reverse order of foreign keys)
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.FieldEdits', 'U') IS NOT NULL DROP TABLE dbo.FieldEdits;
IF OBJECT_ID('dbo.FieldComments', 'U') IS NOT NULL DROP TABLE dbo.FieldComments;
IF OBJECT_ID('dbo.DeleteRequests', 'U') IS NOT NULL DROP TABLE dbo.DeleteRequests;
IF OBJECT_ID('dbo.ReviewNotes', 'U') IS NOT NULL DROP TABLE dbo.ReviewNotes;
IF OBJECT_ID('dbo.AmericanDiplomaStudentTotals', 'U') IS NOT NULL DROP TABLE dbo.AmericanDiplomaStudentTotals;
IF OBJECT_ID('dbo.EmiratiStudentTotals', 'U') IS NOT NULL DROP TABLE dbo.EmiratiStudentTotals;
IF OBJECT_ID('dbo.AzharStudentTotals', 'U') IS NOT NULL DROP TABLE dbo.AzharStudentTotals;
IF OBJECT_ID('dbo.EgyptianStudentTotals', 'U') IS NOT NULL DROP TABLE dbo.EgyptianStudentTotals;
IF OBJECT_ID('dbo.OtherStudentTotals', 'U') IS NOT NULL DROP TABLE dbo.OtherStudentTotals;
IF OBJECT_ID('dbo.PalestinianStudentTotals', 'U') IS NOT NULL DROP TABLE dbo.PalestinianStudentTotals;
IF OBJECT_ID('dbo.BahrainiStudentTotals', 'U') IS NOT NULL DROP TABLE dbo.BahrainiStudentTotals;
IF OBJECT_ID('dbo.YemeniStudentTotals', 'U') IS NOT NULL DROP TABLE dbo.YemeniStudentTotals;
IF OBJECT_ID('dbo.OmaniStudentTotals', 'U') IS NOT NULL DROP TABLE dbo.OmaniStudentTotals;
IF OBJECT_ID('dbo.QatariStudentTotals', 'U') IS NOT NULL DROP TABLE dbo.QatariStudentTotals;
IF OBJECT_ID('dbo.KuwaitiStudentTotals', 'U') IS NOT NULL DROP TABLE dbo.KuwaitiStudentTotals;
IF OBJECT_ID('dbo.StandardStudentGrades', 'U') IS NOT NULL DROP TABLE dbo.StandardStudentGrades;
IF OBJECT_ID('dbo.IGStudentGradeCounts', 'U') IS NOT NULL DROP TABLE dbo.IGStudentGradeCounts;
IF OBJECT_ID('dbo.IGStudentGrades', 'U') IS NOT NULL DROP TABLE dbo.IGStudentGrades;
IF OBJECT_ID('dbo.SaudiStudentGrades', 'U') IS NOT NULL DROP TABLE dbo.SaudiStudentGrades;
IF OBJECT_ID('dbo.SaudiStudentTotals', 'U') IS NOT NULL DROP TABLE dbo.SaudiStudentTotals;
IF OBJECT_ID('dbo.Students', 'U') IS NOT NULL DROP TABLE dbo.Students;
GO

-- 3. Create Students Table (Primary Table)
CREATE TABLE dbo.Students (
    Id INT IDENTITY(1,1) NOT NULL,
    StudentName NVARCHAR(100) NOT NULL,
    StudentNameEn NVARCHAR(100) NOT NULL,
    NationalId NVARCHAR(20) NOT NULL,
    WishCollege NVARCHAR(50) NOT NULL,  -- "الرغبة" — desired college, selection-only
    WishProgram NVARCHAR(100) NULL,     -- desired program, when applicable to the college
    GraduationYear INT NOT NULL,        -- "سنة التخرج" — selection-only, 2022-2026
    Gender NVARCHAR(10) NOT NULL,       -- ذكر / أنثى
    Phone NVARCHAR(20) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    BirthCountry NVARCHAR(100) NOT NULL,
    BirthGovernorate NVARCHAR(100) NOT NULL,
    BirthCity NVARCHAR(100) NOT NULL,
    BirthDate DATE NOT NULL,
    SchoolName NVARCHAR(150) NOT NULL,  -- المدرسة الحاصل منها على الثانوية العامة أو ما يعادلها
    GuardianName NVARCHAR(100) NOT NULL,
    GuardianNationalId NVARCHAR(20) NOT NULL,
    GuardianOccupation NVARCHAR(100) NOT NULL,
    GuardianPhone NVARCHAR(20) NOT NULL,
    GuardianRelation NVARCHAR(100) NOT NULL,
    AddressGov NVARCHAR(100) NOT NULL,
    AddressCenter NVARCHAR(100) NOT NULL,
    AddressVillage NVARCHAR(100) NULL,
    AddressStreet NVARCHAR(200) NOT NULL,
    AddressBuilding NVARCHAR(50) NOT NULL,
    AddressFloor NVARCHAR(20) NULL,
    Certification NVARCHAR(100) NOT NULL,
    Track NVARCHAR(100) NOT NULL,
    PhotoPath NVARCHAR(500) NOT NULL,
    SubmittedAt DATETIME2(7) NOT NULL CONSTRAINT DF_Students_SubmittedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Students PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT UQ_Students_NationalId UNIQUE NONCLUSTERED (NationalId ASC)
);
GO

-- Create Index on NationalId for search optimization
CREATE NONCLUSTERED INDEX IX_Students_NationalId ON dbo.Students (NationalId ASC);
GO

-- 4. Create SaudiStudentTotals Table (One-to-One with Students)
CREATE TABLE dbo.SaudiStudentTotals (
    StudentId INT NOT NULL,
    YearsCount NVARCHAR(50) NOT NULL,
    TotalAchieved DECIMAL(18,2) NOT NULL,
    TotalWeighted DECIMAL(18,2) NOT NULL,
    TotalCoefficients INT NOT NULL,
    SchoolPercentage DECIMAL(18,2) NOT NULL,
    AptitudeScore DECIMAL(18,2) NOT NULL,
    FinalPercentage DECIMAL(18,2) NOT NULL,
    EquivalentTotal DECIMAL(18,2) NOT NULL,  -- المجموع الاعتباري (المجموع المصري) = (FinalPercentage/100)*410
    CONSTRAINT PK_SaudiStudentTotals PRIMARY KEY CLUSTERED (StudentId ASC),
    CONSTRAINT FK_SaudiStudentTotals_Students_StudentId FOREIGN KEY (StudentId) 
        REFERENCES dbo.Students (Id) ON DELETE CASCADE
);
GO

-- 5. Create SaudiStudentGrades Table (One-to-Many with Students)
CREATE TABLE dbo.SaudiStudentGrades (
    Id INT IDENTITY(1,1) NOT NULL,
    StudentId INT NOT NULL,
    YearLabel NVARCHAR(50) NOT NULL, -- e.g., 'Year 1', 'Year 2', 'Year 3'
    SubjectName NVARCHAR(150) NOT NULL,
    Coefficient INT NOT NULL,
    Achieved DECIMAL(18,2) NOT NULL,
    Weighted DECIMAL(18,2) NOT NULL,
    CONSTRAINT PK_SaudiStudentGrades PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT FK_SaudiStudentGrades_Students_StudentId FOREIGN KEY (StudentId) 
        REFERENCES dbo.Students (Id) ON DELETE CASCADE
);
GO

-- Create Index on StudentId and YearLabel for rapid retrieval of reports
CREATE NONCLUSTERED INDEX IX_SaudiStudentGrades_StudentId_YearLabel ON dbo.SaudiStudentGrades (StudentId ASC, YearLabel ASC);
GO

-- 6. Create IGStudentGrades Table (One-to-One with Students)
CREATE TABLE dbo.IGStudentGrades (
    StudentId INT NOT NULL,
    IgProgram NVARCHAR(50) NOT NULL, -- e.g., 'IGCSE', 'AS-Levels', 'A-Levels'
    Factor DECIMAL(18,2) NOT NULL,
    SportsBonus DECIMAL(18,2) NOT NULL,
    ScorePercentage DECIMAL(18,2) NOT NULL,
    GovernmentScore DECIMAL(18,2) NOT NULL,
    CONSTRAINT PK_IGStudentGrades PRIMARY KEY CLUSTERED (StudentId ASC),
    CONSTRAINT FK_IGStudentGrades_Students_StudentId FOREIGN KEY (StudentId) 
        REFERENCES dbo.Students (Id) ON DELETE CASCADE
);
GO

-- 7. Create IGStudentGradeCounts Table (One-to-Many with Students)
CREATE TABLE dbo.IGStudentGradeCounts (
    Id INT IDENTITY(1,1) NOT NULL,
    StudentId INT NOT NULL,
    GradeType NVARCHAR(50) NOT NULL, -- e.g., 'igcse-legacy', 'igcse-numeric', 'as-level', 'a-level'
    Grade NVARCHAR(20) NOT NULL, -- e.g., 'A_STAR', 'A', '9', '8'
    Count INT NOT NULL,
    CONSTRAINT PK_IGStudentGradeCounts PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT FK_IGStudentGradeCounts_Students_StudentId FOREIGN KEY (StudentId) 
        REFERENCES dbo.Students (Id) ON DELETE CASCADE
);
GO

-- Create Index on StudentId
CREATE NONCLUSTERED INDEX IX_IGStudentGradeCounts_StudentId ON dbo.IGStudentGradeCounts (StudentId ASC);
GO

-- 8. Create StandardStudentGrades Table (One-to-Many with Students)
-- GradeLevel/MaxMark are Kuwaiti-only fields (NULL for Qatari/Bahraini rows).
CREATE TABLE dbo.StandardStudentGrades (
    Id INT IDENTITY(1,1) NOT NULL,
    StudentId INT NOT NULL,
    YearOfStudy NVARCHAR(50) NOT NULL,
    SubjectName NVARCHAR(150) NOT NULL,
    Grade DECIMAL(18,2) NOT NULL,
    WeightedPercentage DECIMAL(18,2) NOT NULL,
    Achieved DECIMAL(18,2) NOT NULL,
    GradeLevel INT NULL,
    MaxMark DECIMAL(18,2) NULL,
    CONSTRAINT PK_StandardStudentGrades PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT FK_StandardStudentGrades_Students_StudentId FOREIGN KEY (StudentId)
        REFERENCES dbo.Students (Id) ON DELETE CASCADE
);
GO

-- Create Index on StudentId and YearOfStudy
CREATE NONCLUSTERED INDEX IX_StandardStudentGrades_StudentId_YearOfStudy ON dbo.StandardStudentGrades (StudentId ASC, YearOfStudy ASC);
GO

-- 9. Create KuwaitiStudentTotals Table (One-to-One with Students)
-- Grade10Percentage/Grade10Weight are NULL unless YearsCount = 'Three Years'.
-- Grade11Percentage/Grade11Weight are NULL when YearsCount = 'One Year' (grade 12 only, 100% weight).
-- Weights are entered by the student from their own official certificate, not derived server-side.
CREATE TABLE dbo.KuwaitiStudentTotals (
    StudentId INT NOT NULL,
    YearsCount NVARCHAR(50) NOT NULL, -- 'One Year', 'Two Years', or 'Three Years'
    Grade10Percentage DECIMAL(5,2) NULL,
    Grade10Weight DECIMAL(5,2) NULL,
    Grade11Percentage DECIMAL(5,2) NULL,
    Grade11Weight DECIMAL(5,2) NULL,
    Grade12Percentage DECIMAL(5,2) NOT NULL,
    Grade12Weight DECIMAL(5,2) NOT NULL,
    FinalPercentage DECIMAL(5,2) NOT NULL,
    EquivalentTotal DECIMAL(7,2) NOT NULL,
    HasSecondAttempt BIT NOT NULL,
    CONSTRAINT PK_KuwaitiStudentTotals PRIMARY KEY CLUSTERED (StudentId ASC),
    CONSTRAINT FK_KuwaitiStudentTotals_Students_StudentId FOREIGN KEY (StudentId)
        REFERENCES dbo.Students (Id) ON DELETE CASCADE
);
GO

-- 10. Create QatariStudentTotals Table (One-to-One with Students)
-- No IslamicEducationMark or PrintedTotal/PrintedPercentage fields for Qatari (removed per
-- explicit product decision) — FinalTotal/Percentage are computed from the 7 scientific-track
-- subjects only.
CREATE TABLE dbo.QatariStudentTotals (
    StudentId INT NOT NULL,
    FinalTotal DECIMAL(6,2) NOT NULL,       -- out of 700
    Percentage DECIMAL(5,2) NOT NULL,
    EquivalentTotal DECIMAL(6,2) NOT NULL,  -- المجموع الاعتباري (المجموع المصري) = (Percentage/100)*410
    CONSTRAINT PK_QatariStudentTotals PRIMARY KEY CLUSTERED (StudentId ASC),
    CONSTRAINT FK_QatariStudentTotals_Students_StudentId FOREIGN KEY (StudentId)
        REFERENCES dbo.Students (Id) ON DELETE CASCADE
);
GO

-- 11. Create OmaniStudentTotals Table (One-to-One with Students)
-- Mathematically identical shape to QatariStudentTotals (single grade level, fixed 700
-- denominator) — only the subject list differs. No documentation-only fields.
CREATE TABLE dbo.OmaniStudentTotals (
    StudentId INT NOT NULL,
    FinalTotal DECIMAL(6,2) NOT NULL,       -- out of 700
    Percentage DECIMAL(5,2) NOT NULL,
    EquivalentTotal DECIMAL(6,2) NOT NULL,  -- المجموع الاعتباري (المجموع المصري) = (Percentage/100)*410
    CONSTRAINT PK_OmaniStudentTotals PRIMARY KEY CLUSTERED (StudentId ASC),
    CONSTRAINT FK_OmaniStudentTotals_Students_StudentId FOREIGN KEY (StudentId)
        REFERENCES dbo.Students (Id) ON DELETE CASCADE
);
GO

-- 12. Create YemeniStudentTotals Table (One-to-One with Students)
-- Single grade level, 6 subjects fixed at 100 each, fixed denominator 600 — no excluded subject,
-- no documentation-only fields (matches the trimmed Qatari/Omani shape).
CREATE TABLE dbo.YemeniStudentTotals (
    StudentId INT NOT NULL,
    FinalTotal DECIMAL(6,2) NOT NULL,       -- out of 600
    Percentage DECIMAL(5,2) NOT NULL,
    EquivalentTotal DECIMAL(6,2) NOT NULL,  -- المجموع الاعتباري (المجموع المصري) = (Percentage/100)*410
    CONSTRAINT PK_YemeniStudentTotals PRIMARY KEY CLUSTERED (StudentId ASC),
    CONSTRAINT FK_YemeniStudentTotals_Students_StudentId FOREIGN KEY (StudentId)
        REFERENCES dbo.Students (Id) ON DELETE CASCADE
);
GO

-- 13. Create BahrainiStudentTotals Table (One-to-One with Students)
-- Single grade level (last two years), track-dependent subject list — 7 subjects/700 (علمي) or
-- 8 subjects/800 (أدبي). EquivalentTotal scales Percentage to /410, matching the Kuwaiti/IG formula.
CREATE TABLE dbo.BahrainiStudentTotals (
    StudentId INT NOT NULL,
    Track NVARCHAR(50) NOT NULL,
    FinalTotal DECIMAL(6,2) NOT NULL,
    TotalMax DECIMAL(6,2) NOT NULL,         -- 700 or 800
    Percentage DECIMAL(5,2) NOT NULL,
    EquivalentTotal DECIMAL(6,2) NOT NULL,  -- out of 410
    CONSTRAINT PK_BahrainiStudentTotals PRIMARY KEY CLUSTERED (StudentId ASC),
    CONSTRAINT FK_BahrainiStudentTotals_Students_StudentId FOREIGN KEY (StudentId)
        REFERENCES dbo.Students (Id) ON DELETE CASCADE
);
GO

-- Palestinian Tawjihi: percentage-in only, no subjects/max marks/denominator — the student types
-- their final percentage directly and the site converts it. Branch (علمي/أدبي) is recorded only,
-- never forks the calculation.
CREATE TABLE dbo.PalestinianStudentTotals (
    StudentId INT NOT NULL,
    Percentage DECIMAL(5,2) NOT NULL,
    EquivalentTotal DECIMAL(7,2) NOT NULL,  -- المجموع الاعتباري (المجموع المصري) = (Percentage/100)*410
    Branch NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_PalestinianStudentTotals PRIMARY KEY CLUSTERED (StudentId ASC),
    CONSTRAINT FK_PalestinianStudentTotals_Students_StudentId FOREIGN KEY (StudentId)
        REFERENCES dbo.Students (Id) ON DELETE CASCADE
);
GO

-- "أخرى" (Other): percentage-in only, free-text certificate name, no subjects/track/denominator.
-- No equivalent-total conversion for this certificate — percentage only, by explicit product
-- decision (unlike every other percentage-in certificate in this system).
CREATE TABLE dbo.OtherStudentTotals (
    StudentId INT NOT NULL,
    CertificateName NVARCHAR(200) NOT NULL,
    Percentage DECIMAL(5,2) NOT NULL,
    CONSTRAINT PK_OtherStudentTotals PRIMARY KEY CLUSTERED (StudentId ASC),
    CONSTRAINT FK_OtherStudentTotals_Students_StudentId FOREIGN KEY (StudentId)
        REFERENCES dbo.Students (Id) ON DELETE CASCADE
);
GO

-- الثانوية العامة المصرية: this IS the target Egyptian certificate itself, so there is no
-- equivalent-total conversion. Track + SubjectSystem (قديم/حديث) together determine the exact
-- subject set and each subject's fixed max mark (see EgyptianConstants). Denominator is fixed by
-- subject system alone (320 حديث / 410 قديم), never derived from the visible fields' own max marks.
CREATE TABLE dbo.EgyptianStudentTotals (
    StudentId INT NOT NULL,
    Track NVARCHAR(50) NOT NULL,
    SubjectSystem NVARCHAR(20) NOT NULL,
    FinalTotal DECIMAL(6,2) NOT NULL,
    Denominator DECIMAL(6,2) NOT NULL,
    Percentage DECIMAL(5,2) NOT NULL,
    CONSTRAINT PK_EgyptianStudentTotals PRIMARY KEY CLUSTERED (StudentId ASC),
    CONSTRAINT FK_EgyptianStudentTotals_Students_StudentId FOREIGN KEY (StudentId)
        REFERENCES dbo.Students (Id) ON DELETE CASCADE
);
GO

-- الثانوية الأزهرية: fixed subject list per قسم (علمي/أدبي), no subject-system variant (unlike
-- Egyptian). المواد الشرعية are never modeled at all — nothing to exclude. المجموع الاعتباري
-- (المجموع المصري) = Percentage × 4.1 = (Percentage / 100) × 410.
CREATE TABLE dbo.AzharStudentTotals (
    StudentId INT NOT NULL,
    Section NVARCHAR(20) NOT NULL,
    FinalTotal DECIMAL(6,2) NOT NULL,
    Denominator DECIMAL(6,2) NOT NULL,
    Percentage DECIMAL(5,2) NOT NULL,
    EquivalentTotal DECIMAL(6,2) NOT NULL,
    CONSTRAINT PK_AzharStudentTotals PRIMARY KEY CLUSTERED (StudentId ASC),
    CONSTRAINT FK_AzharStudentTotals_Students_StudentId FOREIGN KEY (StudentId)
        REFERENCES dbo.Students (Id) ON DELETE CASCADE
);
GO

-- الشهادة الإماراتية: single track today. Core subjects (5) always required; optional subjects
-- (الكيمياء/العلوم الصحية/الأحياء) counted only if the student submits a mark for them — the
-- denominator therefore varies (500-800) rather than being fixed like every other single-year cert.
CREATE TABLE dbo.EmiratiStudentTotals (
    StudentId INT NOT NULL,
    FinalTotal DECIMAL(6,2) NOT NULL,
    Denominator DECIMAL(6,2) NOT NULL,
    Percentage DECIMAL(5,2) NOT NULL,
    EquivalentTotal DECIMAL(6,2) NOT NULL,
    CONSTRAINT PK_EmiratiStudentTotals PRIMARY KEY CLUSTERED (StudentId ASC),
    CONSTRAINT FK_EmiratiStudentTotals_Students_StudentId FOREIGN KEY (StudentId)
        REFERENCES dbo.Students (Id) ON DELETE CASCADE
);
GO

-- الدبلومة الأمريكية: NO equivalent-total conversion — admission depends on BasePercentage (from
-- the best 8 subjects, out of 40) + SatI + SatII together, per the college selected in "الرغبة".
-- SAT II is shown for the medical group (اختياري دائمًا) and the engineering group (إلزامي إلا إذا
-- StudiedAdvancedMath) — never for تجارة (SatII/SatIISubject1/SatIISubject2 stay NULL there, and
-- also NULL whenever it was applicable-but-optional and simply left empty).
-- SatIBelowMinimum/SatIIBelowMinimum are informational flags only (1050/1100 thresholds) — never
-- enforced as a rejection.
CREATE TABLE dbo.AmericanDiplomaStudentTotals (
    StudentId INT NOT NULL,
    AverageScore DECIMAL(5,2) NOT NULL,
    BasePercentage DECIMAL(5,2) NOT NULL,
    TestType1 NVARCHAR(10) NOT NULL CONSTRAINT DF_AmericanDiplomaStudentTotals_TestType1 DEFAULT (N'SAT'),
    SatI INT NOT NULL,
    ActComposite INT NULL,
    TestType2 NVARCHAR(10) NULL,
    SatII INT NULL,
    ActMath INT NULL,
    SatIISubject1 NVARCHAR(50) NULL,
    SatIISubject2 NVARCHAR(50) NULL,
    StudiedAdvancedMath BIT NOT NULL,
    SatIBelowMinimum BIT NOT NULL,
    SatIIBelowMinimum BIT NOT NULL,
    CONSTRAINT PK_AmericanDiplomaStudentTotals PRIMARY KEY CLUSTERED (StudentId ASC),
    CONSTRAINT FK_AmericanDiplomaStudentTotals_Students_StudentId FOREIGN KEY (StudentId)
        REFERENCES dbo.Students (Id) ON DELETE CASCADE
);
GO

-- Review notes: reviewer comments/suggested changes attached to individual student fields.
-- Entirely separate from the student data itself — never modifies Students or any certificate
-- table. Each row is an append-only note (edits are new rows, not updates); Author is a fixed
-- "User" placeholder until authentication is implemented.
CREATE TABLE dbo.ReviewNotes (
    Id INT IDENTITY(1,1) NOT NULL,
    StudentId INT NOT NULL,
    FieldName NVARCHAR(150) NOT NULL,
    FieldValueSnapshot NVARCHAR(MAX) NULL,
    ReviewerNote NVARCHAR(MAX) NOT NULL,
    Author NVARCHAR(100) NOT NULL CONSTRAINT DF_ReviewNotes_Author DEFAULT (N'User'),
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ReviewNotes_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT PK_ReviewNotes PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT FK_ReviewNotes_Students_StudentId FOREIGN KEY (StudentId)
        REFERENCES dbo.Students (Id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_ReviewNotes_StudentId_FieldName ON dbo.ReviewNotes (StudentId, FieldName);
GO

-- System users: authentication + role-based access (Viewer/Editor/Admin). Passwords are always
-- stored hashed (PasswordHasher<User>, PBKDF2), never in plain text.
CREATE TABLE dbo.Users (
    Id INT IDENTITY(1,1) NOT NULL,
    Username NVARCHAR(50) NOT NULL,
    PasswordHash NVARCHAR(256) NOT NULL,
    Role NVARCHAR(30) NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
    IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    -- Set only on the seeded root admin ("Mohamed") — its username/password/role can never be
    -- edited or deleted through the Admin UI, by anyone, including itself.
    IsProtected BIT NOT NULL CONSTRAINT DF_Users_IsProtected DEFAULT (0),
    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id ASC)
);
GO
CREATE UNIQUE INDEX IX_Users_Username ON dbo.Users (Username);
GO

-- Student Records Editor: per-field edit audit trail (append-only — every save is a new row).
CREATE TABLE dbo.FieldComments (
    Id INT IDENTITY(1,1) NOT NULL,
    StudentId INT NOT NULL,
    FieldName NVARCHAR(150) NOT NULL,
    FieldSnapshot NVARCHAR(MAX) NULL,
    CommentText NVARCHAR(MAX) NOT NULL,
    Author NVARCHAR(100) NOT NULL CONSTRAINT DF_FieldComments_Author DEFAULT (N'Editor'),
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_FieldComments_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt DATETIME2 NULL,
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_FieldComments_Status DEFAULT (N'unreviewed'),
    CONSTRAINT PK_FieldComments PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT FK_FieldComments_Students_StudentId FOREIGN KEY (StudentId)
        REFERENCES dbo.Students (Id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_FieldComments_StudentId_FieldName ON dbo.FieldComments (StudentId, FieldName);
CREATE INDEX IX_FieldComments_Status ON dbo.FieldComments (Status);
GO

CREATE TABLE dbo.FieldEdits (
    Id INT IDENTITY(1,1) NOT NULL,
    StudentId INT NOT NULL,
    FieldName NVARCHAR(150) NOT NULL,
    OldValue NVARCHAR(MAX) NULL,
    NewValue NVARCHAR(MAX) NULL,
    Editor NVARCHAR(100) NOT NULL CONSTRAINT DF_FieldEdits_Editor DEFAULT (N'Editor'),
    EditedAt DATETIME2 NOT NULL CONSTRAINT DF_FieldEdits_EditedAt DEFAULT (SYSUTCDATETIME()),
    Note NVARCHAR(MAX) NULL,
    Source NVARCHAR(20) NOT NULL CONSTRAINT DF_FieldEdits_Source DEFAULT (N'manual'),
    SourceCommentId INT NULL,
    CONSTRAINT PK_FieldEdits PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT FK_FieldEdits_Students_StudentId FOREIGN KEY (StudentId)
        REFERENCES dbo.Students (Id) ON DELETE CASCADE,
    -- NO ACTION: SQL Server rejects a second cascading path to FieldEdits via FieldComments (which
    -- itself cascades from Students). Both tables are cleared together by the same Students delete.
    CONSTRAINT FK_FieldEdits_FieldComments_SourceCommentId FOREIGN KEY (SourceCommentId)
        REFERENCES dbo.FieldComments (Id) ON DELETE NO ACTION
);
GO
CREATE INDEX IX_FieldEdits_StudentId_FieldName ON dbo.FieldEdits (StudentId, FieldName);
CREATE INDEX IX_FieldEdits_SourceCommentId ON dbo.FieldEdits (SourceCommentId);
GO

-- Editor-requested student deletions — Editors can never delete a student row directly; only an
-- Admin approving this request does. StudentId is nullable + SET NULL (not cascade) so this audit
-- row survives the student's actual deletion as proof the approval happened.
CREATE TABLE dbo.DeleteRequests (
    Id INT IDENTITY(1,1) NOT NULL,
    StudentId INT NULL,
    RequestedBy NVARCHAR(100) NOT NULL CONSTRAINT DF_DeleteRequests_RequestedBy DEFAULT (N'Editor'),
    RequestedAt DATETIME2 NOT NULL CONSTRAINT DF_DeleteRequests_RequestedAt DEFAULT (SYSUTCDATETIME()),
    Reason NVARCHAR(MAX) NULL,
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_DeleteRequests_Status DEFAULT (N'pending'),
    ReviewedBy NVARCHAR(100) NULL,
    ReviewedAt DATETIME2 NULL,
    CONSTRAINT PK_DeleteRequests PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT FK_DeleteRequests_Students_StudentId FOREIGN KEY (StudentId)
        REFERENCES dbo.Students (Id) ON DELETE SET NULL
);
GO
CREATE INDEX IX_DeleteRequests_StudentId ON dbo.DeleteRequests (StudentId);
CREATE INDEX IX_DeleteRequests_Status ON dbo.DeleteRequests (Status);
GO
