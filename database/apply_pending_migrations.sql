IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723002919_InitialCreate'
)
BEGIN
    CREATE TABLE [dbo].[Students] (
        [Id] int NOT NULL IDENTITY,
        [StudentName] nvarchar(100) NOT NULL,
        [StudentNameEn] nvarchar(100) NOT NULL,
        [NationalId] nvarchar(20) NOT NULL,
        [Phone] nvarchar(20) NOT NULL,
        [Email] nvarchar(150) NOT NULL,
        [GuardianName] nvarchar(100) NOT NULL,
        [GuardianPhone] nvarchar(20) NOT NULL,
        [GuardianRelation] nvarchar(100) NOT NULL,
        [AddressGov] nvarchar(100) NOT NULL,
        [AddressCenter] nvarchar(100) NOT NULL,
        [AddressVillage] nvarchar(100) NULL,
        [AddressStreet] nvarchar(200) NOT NULL,
        [AddressBuilding] nvarchar(50) NOT NULL,
        [AddressFloor] nvarchar(20) NULL,
        [Certification] nvarchar(100) NOT NULL,
        [Track] nvarchar(100) NOT NULL,
        [PhotoPath] nvarchar(500) NOT NULL,
        [SubmittedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Students] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723002919_InitialCreate'
)
BEGIN
    CREATE TABLE [dbo].[IGStudentGradeCounts] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [GradeType] nvarchar(50) NOT NULL,
        [Grade] nvarchar(20) NOT NULL,
        [Count] int NOT NULL,
        CONSTRAINT [PK_IGStudentGradeCounts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_IGStudentGradeCounts_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723002919_InitialCreate'
)
BEGIN
    CREATE TABLE [dbo].[IGStudentGrades] (
        [StudentId] int NOT NULL,
        [IgProgram] nvarchar(50) NOT NULL,
        [Factor] decimal(18,2) NOT NULL,
        [SportsBonus] decimal(18,2) NOT NULL,
        [ScorePercentage] decimal(18,2) NOT NULL,
        [GovernmentScore] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_IGStudentGrades] PRIMARY KEY ([StudentId]),
        CONSTRAINT [FK_IGStudentGrades_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723002919_InitialCreate'
)
BEGIN
    CREATE TABLE [dbo].[SaudiStudentGrades] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [YearLabel] nvarchar(50) NOT NULL,
        [SubjectName] nvarchar(150) NOT NULL,
        [Coefficient] int NOT NULL,
        [Achieved] decimal(18,2) NOT NULL,
        [Weighted] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_SaudiStudentGrades] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SaudiStudentGrades_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723002919_InitialCreate'
)
BEGIN
    CREATE TABLE [dbo].[SaudiStudentTotals] (
        [StudentId] int NOT NULL,
        [YearsCount] nvarchar(50) NOT NULL,
        [TotalAchieved] decimal(18,2) NOT NULL,
        [TotalWeighted] decimal(18,2) NOT NULL,
        [TotalCoefficients] int NOT NULL,
        [FinalPercentage] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_SaudiStudentTotals] PRIMARY KEY ([StudentId]),
        CONSTRAINT [FK_SaudiStudentTotals_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723002919_InitialCreate'
)
BEGIN
    CREATE TABLE [dbo].[StandardStudentGrades] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [YearOfStudy] nvarchar(50) NOT NULL,
        [SubjectName] nvarchar(150) NOT NULL,
        [Grade] decimal(18,2) NOT NULL,
        [WeightedPercentage] decimal(18,2) NOT NULL,
        [Achieved] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_StandardStudentGrades] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StandardStudentGrades_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723002919_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_IGStudentGradeCounts_StudentId] ON [dbo].[IGStudentGradeCounts] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723002919_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SaudiStudentGrades_StudentId_YearLabel] ON [dbo].[SaudiStudentGrades] ([StudentId], [YearLabel]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723002919_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StandardStudentGrades_StudentId_YearOfStudy] ON [dbo].[StandardStudentGrades] ([StudentId], [YearOfStudy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723002919_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Students_NationalId] ON [dbo].[Students] ([NationalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723002919_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260723002919_InitialCreate', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723173010_AddKuwaitiCertificateSupport'
)
BEGIN
    ALTER TABLE [dbo].[StandardStudentGrades] ADD [GradeLevel] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723173010_AddKuwaitiCertificateSupport'
)
BEGIN
    ALTER TABLE [dbo].[StandardStudentGrades] ADD [MaxMark] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723173010_AddKuwaitiCertificateSupport'
)
BEGIN
    CREATE TABLE [dbo].[KuwaitiStudentTotals] (
        [StudentId] int NOT NULL,
        [YearsCount] nvarchar(50) NOT NULL,
        [Grade10Percentage] decimal(5,2) NULL,
        [Grade10Weight] decimal(5,2) NULL,
        [Grade11Percentage] decimal(5,2) NULL,
        [Grade11Weight] decimal(5,2) NULL,
        [Grade12Percentage] decimal(5,2) NOT NULL,
        [Grade12Weight] decimal(5,2) NOT NULL,
        [FinalPercentage] decimal(5,2) NOT NULL,
        [EquivalentTotal] decimal(7,2) NOT NULL,
        [HasSecondAttempt] bit NOT NULL,
        CONSTRAINT [PK_KuwaitiStudentTotals] PRIMARY KEY ([StudentId]),
        CONSTRAINT [FK_KuwaitiStudentTotals_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723173010_AddKuwaitiCertificateSupport'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260723173010_AddKuwaitiCertificateSupport', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723183148_AddQatariCertificateSupport'
)
BEGIN
    CREATE TABLE [dbo].[QatariStudentTotals] (
        [StudentId] int NOT NULL,
        [FinalTotal] decimal(6,2) NOT NULL,
        [Percentage] decimal(5,2) NOT NULL,
        [IslamicEducationMark] decimal(5,2) NULL,
        [PrintedTotal] decimal(6,2) NULL,
        [PrintedPercentage] decimal(5,2) NULL,
        CONSTRAINT [PK_QatariStudentTotals] PRIMARY KEY ([StudentId]),
        CONSTRAINT [FK_QatariStudentTotals_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723183148_AddQatariCertificateSupport'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260723183148_AddQatariCertificateSupport', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723201200_AddSaudiAptitudeScore'
)
BEGIN
    ALTER TABLE [dbo].[SaudiStudentTotals] ADD [AptitudeScore] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723201200_AddSaudiAptitudeScore'
)
BEGIN
    ALTER TABLE [dbo].[SaudiStudentTotals] ADD [SchoolPercentage] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723201200_AddSaudiAptitudeScore'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260723201200_AddSaudiAptitudeScore', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723201640_AddOmaniCertificateSupport'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[dbo].[QatariStudentTotals]') AND [c].[name] = N'IslamicEducationMark');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [dbo].[QatariStudentTotals] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [dbo].[QatariStudentTotals] DROP COLUMN [IslamicEducationMark];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723201640_AddOmaniCertificateSupport'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[dbo].[QatariStudentTotals]') AND [c].[name] = N'PrintedPercentage');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [dbo].[QatariStudentTotals] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [dbo].[QatariStudentTotals] DROP COLUMN [PrintedPercentage];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723201640_AddOmaniCertificateSupport'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[dbo].[QatariStudentTotals]') AND [c].[name] = N'PrintedTotal');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [dbo].[QatariStudentTotals] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [dbo].[QatariStudentTotals] DROP COLUMN [PrintedTotal];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723201640_AddOmaniCertificateSupport'
)
BEGIN
    CREATE TABLE [dbo].[OmaniStudentTotals] (
        [StudentId] int NOT NULL,
        [FinalTotal] decimal(6,2) NOT NULL,
        [Percentage] decimal(5,2) NOT NULL,
        CONSTRAINT [PK_OmaniStudentTotals] PRIMARY KEY ([StudentId]),
        CONSTRAINT [FK_OmaniStudentTotals_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723201640_AddOmaniCertificateSupport'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260723201640_AddOmaniCertificateSupport', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724004232_AddYemeniCertificateSupport'
)
BEGIN
    CREATE TABLE [dbo].[YemeniStudentTotals] (
        [StudentId] int NOT NULL,
        [FinalTotal] decimal(6,2) NOT NULL,
        [Percentage] decimal(5,2) NOT NULL,
        CONSTRAINT [PK_YemeniStudentTotals] PRIMARY KEY ([StudentId]),
        CONSTRAINT [FK_YemeniStudentTotals_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724004232_AddYemeniCertificateSupport'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260724004232_AddYemeniCertificateSupport', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724130419_AddBahrainiCertificateSupport'
)
BEGIN
    CREATE TABLE [dbo].[BahrainiStudentTotals] (
        [StudentId] int NOT NULL,
        [Track] nvarchar(50) NOT NULL,
        [FinalTotal] decimal(6,2) NOT NULL,
        [TotalMax] decimal(6,2) NOT NULL,
        [Percentage] decimal(5,2) NOT NULL,
        [EquivalentTotal] decimal(6,2) NOT NULL,
        CONSTRAINT [PK_BahrainiStudentTotals] PRIMARY KEY ([StudentId]),
        CONSTRAINT [FK_BahrainiStudentTotals_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724130419_AddBahrainiCertificateSupport'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260724130419_AddBahrainiCertificateSupport', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724162215_AddSaudiAndOmaniEquivalentTotal'
)
BEGIN
    ALTER TABLE [dbo].[SaudiStudentTotals] ADD [EquivalentTotal] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724162215_AddSaudiAndOmaniEquivalentTotal'
)
BEGIN
    ALTER TABLE [dbo].[OmaniStudentTotals] ADD [EquivalentTotal] decimal(6,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724162215_AddSaudiAndOmaniEquivalentTotal'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260724162215_AddSaudiAndOmaniEquivalentTotal', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724172222_AddQatariEquivalentTotal'
)
BEGIN
    ALTER TABLE [dbo].[QatariStudentTotals] ADD [EquivalentTotal] decimal(6,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724172222_AddQatariEquivalentTotal'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260724172222_AddQatariEquivalentTotal', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724172640_AddYemeniEquivalentTotal'
)
BEGIN
    ALTER TABLE [dbo].[YemeniStudentTotals] ADD [EquivalentTotal] decimal(6,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724172640_AddYemeniEquivalentTotal'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260724172640_AddYemeniEquivalentTotal', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724183307_AddPalestinianCertificateSupport'
)
BEGIN
    CREATE TABLE [dbo].[PalestinianStudentTotals] (
        [StudentId] int NOT NULL,
        [Percentage] decimal(5,2) NOT NULL,
        [EquivalentTotal] decimal(7,2) NOT NULL,
        [Branch] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_PalestinianStudentTotals] PRIMARY KEY ([StudentId]),
        CONSTRAINT [FK_PalestinianStudentTotals_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724183307_AddPalestinianCertificateSupport'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260724183307_AddPalestinianCertificateSupport', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724213539_AddOtherCertificateSupport'
)
BEGIN
    CREATE TABLE [dbo].[OtherStudentTotals] (
        [StudentId] int NOT NULL,
        [CertificateName] nvarchar(200) NOT NULL,
        [Percentage] decimal(5,2) NOT NULL,
        CONSTRAINT [PK_OtherStudentTotals] PRIMARY KEY ([StudentId]),
        CONSTRAINT [FK_OtherStudentTotals_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724213539_AddOtherCertificateSupport'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260724213539_AddOtherCertificateSupport', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724214801_AddStudentGenderAndGuardianDetails'
)
BEGIN
    ALTER TABLE [dbo].[Students] ADD [Gender] nvarchar(10) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724214801_AddStudentGenderAndGuardianDetails'
)
BEGIN
    ALTER TABLE [dbo].[Students] ADD [GuardianNationalId] nvarchar(20) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724214801_AddStudentGenderAndGuardianDetails'
)
BEGIN
    ALTER TABLE [dbo].[Students] ADD [GuardianOccupation] nvarchar(100) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724214801_AddStudentGenderAndGuardianDetails'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260724214801_AddStudentGenderAndGuardianDetails', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724222341_AddStudentWishCollegeAndProgram'
)
BEGIN
    ALTER TABLE [dbo].[Students] ADD [WishCollege] nvarchar(50) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724222341_AddStudentWishCollegeAndProgram'
)
BEGIN
    ALTER TABLE [dbo].[Students] ADD [WishProgram] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724222341_AddStudentWishCollegeAndProgram'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260724222341_AddStudentWishCollegeAndProgram', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724224027_AddStudentGraduationYear'
)
BEGIN
    ALTER TABLE [dbo].[Students] ADD [GraduationYear] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724224027_AddStudentGraduationYear'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260724224027_AddStudentGraduationYear', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724232502_AddEgyptianCertificateSupport'
)
BEGIN
    CREATE TABLE [dbo].[EgyptianStudentTotals] (
        [StudentId] int NOT NULL,
        [Track] nvarchar(50) NOT NULL,
        [SubjectSystem] nvarchar(20) NOT NULL,
        [FinalTotal] decimal(6,2) NOT NULL,
        [Denominator] decimal(6,2) NOT NULL,
        [Percentage] decimal(5,2) NOT NULL,
        CONSTRAINT [PK_EgyptianStudentTotals] PRIMARY KEY ([StudentId]),
        CONSTRAINT [FK_EgyptianStudentTotals_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260724232502_AddEgyptianCertificateSupport'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260724232502_AddEgyptianCertificateSupport', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725093950_AddAzharCertificateSupport'
)
BEGIN
    CREATE TABLE [dbo].[AzharStudentTotals] (
        [StudentId] int NOT NULL,
        [Section] nvarchar(20) NOT NULL,
        [FinalTotal] decimal(6,2) NOT NULL,
        [Denominator] decimal(6,2) NOT NULL,
        [Percentage] decimal(5,2) NOT NULL,
        [EquivalentTotal] decimal(6,2) NOT NULL,
        CONSTRAINT [PK_AzharStudentTotals] PRIMARY KEY ([StudentId]),
        CONSTRAINT [FK_AzharStudentTotals_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725093950_AddAzharCertificateSupport'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260725093950_AddAzharCertificateSupport', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725122418_AddEmiratiCertificateSupport'
)
BEGIN
    CREATE TABLE [dbo].[EmiratiStudentTotals] (
        [StudentId] int NOT NULL,
        [FinalTotal] decimal(6,2) NOT NULL,
        [Denominator] decimal(6,2) NOT NULL,
        [Percentage] decimal(5,2) NOT NULL,
        [EquivalentTotal] decimal(6,2) NOT NULL,
        CONSTRAINT [PK_EmiratiStudentTotals] PRIMARY KEY ([StudentId]),
        CONSTRAINT [FK_EmiratiStudentTotals_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725122418_AddEmiratiCertificateSupport'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260725122418_AddEmiratiCertificateSupport', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725134758_AddAmericanDiplomaCertificateSupport'
)
BEGIN
    CREATE TABLE [dbo].[AmericanDiplomaStudentTotals] (
        [StudentId] int NOT NULL,
        [AverageScore] decimal(5,2) NOT NULL,
        [BasePercentage] decimal(5,2) NOT NULL,
        [SatI] int NOT NULL,
        [SatII] int NULL,
        [SatIISubject1] nvarchar(50) NULL,
        [SatIISubject2] nvarchar(50) NULL,
        [SatIBelowMinimum] bit NOT NULL,
        [SatIIBelowMinimum] bit NOT NULL,
        CONSTRAINT [PK_AmericanDiplomaStudentTotals] PRIMARY KEY ([StudentId]),
        CONSTRAINT [FK_AmericanDiplomaStudentTotals_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725134758_AddAmericanDiplomaCertificateSupport'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260725134758_AddAmericanDiplomaCertificateSupport', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725151029_AddAmericanDiplomaStudiedAdvancedMath'
)
BEGIN
    ALTER TABLE [dbo].[AmericanDiplomaStudentTotals] ADD [StudiedAdvancedMath] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725151029_AddAmericanDiplomaStudiedAdvancedMath'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260725151029_AddAmericanDiplomaStudiedAdvancedMath', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725195214_AddReviewNotes'
)
BEGIN
    CREATE TABLE [dbo].[ReviewNotes] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [FieldName] nvarchar(150) NOT NULL,
        [FieldValueSnapshot] nvarchar(max) NULL,
        [ReviewerNote] nvarchar(max) NOT NULL,
        [Author] nvarchar(100) NOT NULL DEFAULT N'User',
        [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ReviewNotes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ReviewNotes_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725195214_AddReviewNotes'
)
BEGIN
    CREATE INDEX [IX_ReviewNotes_StudentId_FieldName] ON [dbo].[ReviewNotes] ([StudentId], [FieldName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725195214_AddReviewNotes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260725195214_AddReviewNotes', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726052931_AddStudentBirthAndSchoolDetails'
)
BEGIN
    ALTER TABLE [dbo].[Students] ADD [BirthCity] nvarchar(100) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726052931_AddStudentBirthAndSchoolDetails'
)
BEGIN
    ALTER TABLE [dbo].[Students] ADD [BirthCountry] nvarchar(100) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726052931_AddStudentBirthAndSchoolDetails'
)
BEGIN
    ALTER TABLE [dbo].[Students] ADD [BirthDate] date NOT NULL DEFAULT '0001-01-01';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726052931_AddStudentBirthAndSchoolDetails'
)
BEGIN
    ALTER TABLE [dbo].[Students] ADD [BirthGovernorate] nvarchar(100) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726052931_AddStudentBirthAndSchoolDetails'
)
BEGIN
    ALTER TABLE [dbo].[Students] ADD [SchoolName] nvarchar(150) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726052931_AddStudentBirthAndSchoolDetails'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260726052931_AddStudentBirthAndSchoolDetails', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726062722_AddAmericanDiplomaActSupport'
)
BEGIN
    ALTER TABLE [dbo].[AmericanDiplomaStudentTotals] ADD [ActComposite] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726062722_AddAmericanDiplomaActSupport'
)
BEGIN
    ALTER TABLE [dbo].[AmericanDiplomaStudentTotals] ADD [ActMath] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726062722_AddAmericanDiplomaActSupport'
)
BEGIN
    ALTER TABLE [dbo].[AmericanDiplomaStudentTotals] ADD [TestType1] nvarchar(10) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726062722_AddAmericanDiplomaActSupport'
)
BEGIN
    ALTER TABLE [dbo].[AmericanDiplomaStudentTotals] ADD [TestType2] nvarchar(10) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726062722_AddAmericanDiplomaActSupport'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260726062722_AddAmericanDiplomaActSupport', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726072159_AddUsersTable'
)
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id] int NOT NULL IDENTITY,
        [Username] nvarchar(50) NOT NULL,
        [PasswordHash] nvarchar(256) NOT NULL,
        [Role] nvarchar(30) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726072159_AddUsersTable'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Username] ON [dbo].[Users] ([Username]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726072159_AddUsersTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260726072159_AddUsersTable', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726170502_AddGuardianLandlinePhone'
)
BEGIN
    ALTER TABLE [dbo].[Students] ADD [GuardianLandlinePhone] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726170502_AddGuardianLandlinePhone'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260726170502_AddGuardianLandlinePhone', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726185032_AddEditorTables'
)
BEGIN
    CREATE TABLE [dbo].[DeleteRequests] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] int NULL,
        [RequestedBy] nvarchar(100) NOT NULL DEFAULT N'Editor',
        [RequestedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [Reason] nvarchar(max) NULL,
        [Status] nvarchar(20) NOT NULL DEFAULT N'pending',
        [ReviewedBy] nvarchar(100) NULL,
        [ReviewedAt] datetime2 NULL,
        CONSTRAINT [PK_DeleteRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DeleteRequests_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726185032_AddEditorTables'
)
BEGIN
    CREATE TABLE [dbo].[FieldComments] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [FieldName] nvarchar(150) NOT NULL,
        [FieldSnapshot] nvarchar(max) NULL,
        [CommentText] nvarchar(max) NOT NULL,
        [Author] nvarchar(100) NOT NULL DEFAULT N'Editor',
        [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] datetime2 NULL,
        [Status] nvarchar(20) NOT NULL DEFAULT N'unreviewed',
        CONSTRAINT [PK_FieldComments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FieldComments_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726185032_AddEditorTables'
)
BEGIN
    CREATE TABLE [dbo].[FieldEdits] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [FieldName] nvarchar(150) NOT NULL,
        [OldValue] nvarchar(max) NULL,
        [NewValue] nvarchar(max) NULL,
        [Editor] nvarchar(100) NOT NULL DEFAULT N'Editor',
        [EditedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [Note] nvarchar(max) NULL,
        [Source] nvarchar(20) NOT NULL DEFAULT N'manual',
        [SourceCommentId] int NULL,
        CONSTRAINT [PK_FieldEdits] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FieldEdits_FieldComments_SourceCommentId] FOREIGN KEY ([SourceCommentId]) REFERENCES [dbo].[FieldComments] ([Id]),
        CONSTRAINT [FK_FieldEdits_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726185032_AddEditorTables'
)
BEGIN
    CREATE INDEX [IX_DeleteRequests_Status] ON [dbo].[DeleteRequests] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726185032_AddEditorTables'
)
BEGIN
    CREATE INDEX [IX_DeleteRequests_StudentId] ON [dbo].[DeleteRequests] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726185032_AddEditorTables'
)
BEGIN
    CREATE INDEX [IX_FieldComments_Status] ON [dbo].[FieldComments] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726185032_AddEditorTables'
)
BEGIN
    CREATE INDEX [IX_FieldComments_StudentId_FieldName] ON [dbo].[FieldComments] ([StudentId], [FieldName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726185032_AddEditorTables'
)
BEGIN
    CREATE INDEX [IX_FieldEdits_SourceCommentId] ON [dbo].[FieldEdits] ([SourceCommentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726185032_AddEditorTables'
)
BEGIN
    CREATE INDEX [IX_FieldEdits_StudentId_FieldName] ON [dbo].[FieldEdits] ([StudentId], [FieldName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726185032_AddEditorTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260726185032_AddEditorTables', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726214453_AddAmericanDiplomaEquivalentPercentage'
)
BEGIN
    ALTER TABLE [dbo].[AmericanDiplomaStudentTotals] ADD [EquivalentPercentage] decimal(6,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726214453_AddAmericanDiplomaEquivalentPercentage'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260726214453_AddAmericanDiplomaEquivalentPercentage', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727134709_AddUserIsProtected'
)
BEGIN
    ALTER TABLE [dbo].[Users] ADD [IsProtected] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727134709_AddUserIsProtected'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260727134709_AddUserIsProtected', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727155714_AddStudentSubmissionToken'
)
BEGIN
    ALTER TABLE [dbo].[Students] ADD [SubmissionToken] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727155714_AddStudentSubmissionToken'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Students_SubmissionToken] ON [dbo].[Students] ([SubmissionToken]) WHERE [SubmissionToken] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727155714_AddStudentSubmissionToken'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260727155714_AddStudentSubmissionToken', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727212350_AddPendingReviews'
)
BEGIN
    CREATE TABLE [dbo].[PendingReviews] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [FlaggedBy] nvarchar(100) NOT NULL DEFAULT N'User',
        [FlaggedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [Status] nvarchar(20) NOT NULL DEFAULT N'pending',
        [ResolvedBy] nvarchar(100) NULL,
        [ResolvedAt] datetime2 NULL,
        CONSTRAINT [PK_PendingReviews] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PendingReviews_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[Students] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727212350_AddPendingReviews'
)
BEGIN
    CREATE INDEX [IX_PendingReviews_Status] ON [dbo].[PendingReviews] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727212350_AddPendingReviews'
)
BEGIN
    CREATE INDEX [IX_PendingReviews_StudentId] ON [dbo].[PendingReviews] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727212350_AddPendingReviews'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260727212350_AddPendingReviews', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727220921_AddStudentEligibilityStatus'
)
BEGIN
    ALTER TABLE [dbo].[Students] ADD [EligibilityConfirmedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727220921_AddStudentEligibilityStatus'
)
BEGIN
    ALTER TABLE [dbo].[Students] ADD [EligibilityConfirmedBy] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727220921_AddStudentEligibilityStatus'
)
BEGIN
    ALTER TABLE [dbo].[Students] ADD [EligibilityStatus] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727220921_AddStudentEligibilityStatus'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260727220921_AddStudentEligibilityStatus', N'8.0.29');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728074933_AddStudentEligibilityNote'
)
BEGIN
    ALTER TABLE [dbo].[Students] ADD [EligibilityNote] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728074933_AddStudentEligibilityNote'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260728074933_AddStudentEligibilityNote', N'8.0.29');
END;
GO

COMMIT;
GO

