-- Seeds the 3 default system users (Viewer / Editor / Admin) into dbo.Users.
--
-- The app itself already does this automatically on first run (see Program.cs — it seeds once,
-- only when dbo.Users is completely empty). Use this script instead when you need the users in
-- the database WITHOUT starting the app first (e.g. right after applying migrations / running
-- schema.sql on a fresh database, or for a deployment/CI step).
--
-- Idempotent: safe to re-run — each INSERT only fires if that username doesn't already exist,
-- so running this after the app has already seeded (or after you've added other users) does
-- nothing and never creates duplicates or overwrites an existing password.
--
-- Passwords are never stored in plain text. The hashes below were generated with ASP.NET Core's
-- own PasswordHasher<T> (the exact same class AuthService.cs uses to verify logins) for the
-- password "1234" — change these accounts' passwords through the app once real users take over.

SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = N'viewer')
BEGIN
    INSERT INTO dbo.Users (Username, PasswordHash, Role, CreatedAt, IsActive)
    VALUES (N'viewer', N'AQAAAAIAAYagAAAAEC6ox1bcfV3bdL1cMABxd5FyRlh0mQWhE509ALtpAOImCbW/WC/MF9CbKfyVd/chWg==', N'Viewer', SYSUTCDATETIME(), 1);
END

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = N'editor')
BEGIN
    INSERT INTO dbo.Users (Username, PasswordHash, Role, CreatedAt, IsActive)
    VALUES (N'editor', N'AQAAAAIAAYagAAAAEEKSBNSQCZXfZtU2cPYNJh51N56eW2emhZgHjpPDpd40UhnrAfRr5ropxj3BqxfPrw==', N'Editor', SYSUTCDATETIME(), 1);
END

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = N'admin')
BEGIN
    INSERT INTO dbo.Users (Username, PasswordHash, Role, CreatedAt, IsActive)
    VALUES (N'admin', N'AQAAAAIAAYagAAAAEJQei6wGENwwc2psK5JhnhUsY8Q9QWP6gwtU/o76R1v9r1e3jMEDRHLTYZkUwCFc9A==', N'Admin', SYSUTCDATETIME(), 1);
END

SELECT Id, Username, Role, IsActive, CreatedAt FROM dbo.Users ORDER BY Id;
