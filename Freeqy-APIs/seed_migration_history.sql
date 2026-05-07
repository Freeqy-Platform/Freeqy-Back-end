-- Seed the EF Migrations History table with all previously-applied migrations
-- This fixes the "all migrations Pending" issue when the __EFMigrationsHistory table is empty
-- but the database tables already exist.

-- First, ensure the table exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END
GO

-- Insert all existing migrations (that were applied before the history was lost)
-- Using the EF Core 9 product version
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
SELECT m.[MigrationId], N'9.0.0'
FROM (VALUES
    (N'20251124201130_AddPhotoUrlToUser'),
    (N'20251201190210_UpdateUserSocialMediaTable'),
    (N'20251204021823_AddUserEducations'),
    (N'20251204035610_AddUserCertificates'),
    (N'20251206003600_AddUserSummary'),
    (N'20251206005306_AddUserAvailability'),
    (N'20251219232015_AddTrackRequestSystem'),
    (N'20251220010506_AddBannerPhotoToApplicationUser'),
    (N'20260305071633_AddMessagingFeature')
) AS m([MigrationId])
WHERE NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory] h WHERE h.[MigrationId] = m.[MigrationId]
);
GO

PRINT 'Migration history seeded successfully. You can now run: dotnet ef database update';
GO
