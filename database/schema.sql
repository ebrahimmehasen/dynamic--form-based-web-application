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
-- SatII/SatIISubject1/SatIISubject2 are NULL for colleges that don't require it (تجارة).
-- SatIBelowMinimum/SatIIBelowMinimum are informational flags only (1050/1100 thresholds) — never
-- enforced as a rejection.
CREATE TABLE dbo.AmericanDiplomaStudentTotals (
    StudentId INT NOT NULL,
    AverageScore DECIMAL(5,2) NOT NULL,
    BasePercentage DECIMAL(5,2) NOT NULL,
    SatI INT NOT NULL,
    SatII INT NULL,
    SatIISubject1 NVARCHAR(50) NULL,
    SatIISubject2 NVARCHAR(50) NULL,
    SatIBelowMinimum BIT NOT NULL,
    SatIIBelowMinimum BIT NOT NULL,
    CONSTRAINT PK_AmericanDiplomaStudentTotals PRIMARY KEY CLUSTERED (StudentId ASC),
    CONSTRAINT FK_AmericanDiplomaStudentTotals_Students_StudentId FOREIGN KEY (StudentId)
        REFERENCES dbo.Students (Id) ON DELETE CASCADE
);
GO
