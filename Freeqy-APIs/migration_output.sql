BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507173638_AddNotificationSystem'
)
BEGIN
    ALTER TABLE [ConversationParticipants] ADD [IsMuted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507173638_AddNotificationSystem'
)
BEGIN

                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Badges')
                    BEGIN
                        CREATE TABLE [Badges] (
                            [Id] int NOT NULL IDENTITY,
                            [Type] int NOT NULL,
                            [Name] nvarchar(max) NOT NULL,
                            [Description] nvarchar(max) NOT NULL,
                            CONSTRAINT [PK_Badges] PRIMARY KEY ([Id])
                        );

                        SET IDENTITY_INSERT [Badges] ON;
                        INSERT INTO [Badges] ([Id], [Description], [Name], [Type]) VALUES
                            (1, N'Created your first project', N'First Project', 1),
                            (2, N'Joined 5 projects as a member', N'Team Player', 2),
                            (3, N'Participated in 3 completed projects', N'Project Completer', 3),
                            (4, N'One of the first 100 users on Freeqy', N'Early Adopter', 4),
                            (5, N'Owned and led 5 projects', N'Mentor', 5),
                            (6, N'Achieved a contribution score over 500', N'Top Contributor', 6);
                        SET IDENTITY_INSERT [Badges] OFF;
                    END
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507173638_AddNotificationSystem'
)
BEGIN

                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserBadges')
                    BEGIN
                        CREATE TABLE [UserBadges] (
                            [UserId] nvarchar(450) NOT NULL,
                            [BadgeId] int NOT NULL,
                            [EarnedAt] datetime2 NOT NULL,
                            CONSTRAINT [PK_UserBadges] PRIMARY KEY ([UserId], [BadgeId]),
                            CONSTRAINT [FK_UserBadges_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
                            CONSTRAINT [FK_UserBadges_Badges_BadgeId] FOREIGN KEY ([BadgeId]) REFERENCES [Badges] ([Id]) ON DELETE CASCADE
                        );
                        CREATE INDEX [IX_UserBadges_BadgeId] ON [UserBadges] ([BadgeId]);
                    END
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507173638_AddNotificationSystem'
)
BEGIN

                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserProjectHistories')
                    BEGIN
                        CREATE TABLE [UserProjectHistories] (
                            [Id] bigint NOT NULL IDENTITY,
                            [UserId] nvarchar(450) NOT NULL,
                            [ProjectId] nvarchar(100) NOT NULL,
                            [ProjectName] nvarchar(200) NOT NULL,
                            [ProjectCategory] nvarchar(100) NOT NULL,
                            [EventType] nvarchar(30) NOT NULL,
                            [Role] nvarchar(50) NULL,
                            [EventDate] datetime2 NOT NULL,
                            [ProjectStatusAtEvent] nvarchar(30) NULL,
                            CONSTRAINT [PK_UserProjectHistories] PRIMARY KEY ([Id]),
                            CONSTRAINT [FK_UserProjectHistories_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
                            CONSTRAINT [FK_UserProjectHistories_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([Id]) ON DELETE NO ACTION
                        );
                        CREATE INDEX [IX_UserProjectHistories_ProjectId] ON [UserProjectHistories] ([ProjectId]);
                        CREATE INDEX [IX_UserProjectHistories_UserId] ON [UserProjectHistories] ([UserId]);
                        CREATE INDEX [IX_UserProjectHistories_UserId_EventDate] ON [UserProjectHistories] ([UserId] ASC, [EventDate] DESC);
                    END
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507173638_AddNotificationSystem'
)
BEGIN
    CREATE TABLE [NotificationPreferences] (
        [UserId] nvarchar(450) NOT NULL,
        [Type] nvarchar(64) NOT NULL,
        [InAppEnabled] bit NOT NULL,
        [EmailEnabled] bit NOT NULL,
        CONSTRAINT [PK_NotificationPreferences] PRIMARY KEY ([UserId], [Type]),
        CONSTRAINT [FK_NotificationPreferences_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507173638_AddNotificationSystem'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] nvarchar(450) NOT NULL,
        [RecipientId] nvarchar(450) NOT NULL,
        [ActorId] nvarchar(450) NULL,
        [Type] nvarchar(64) NOT NULL,
        [Priority] nvarchar(16) NOT NULL,
        [Title] nvarchar(256) NOT NULL,
        [Message] nvarchar(1024) NOT NULL,
        [EntityType] nvarchar(64) NULL,
        [EntityId] nvarchar(128) NULL,
        [ActionUrl] nvarchar(512) NULL,
        [IsRead] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ReadAt] datetime2 NULL,
        [EmailSent] bit NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_AspNetUsers_ActorId] FOREIGN KEY ([ActorId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_Notifications_AspNetUsers_RecipientId] FOREIGN KEY ([RecipientId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507173638_AddNotificationSystem'
)
BEGIN
    CREATE INDEX [IX_Notifications_ActorId] ON [Notifications] ([ActorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507173638_AddNotificationSystem'
)
BEGIN
    CREATE INDEX [IX_Notifications_CreatedAt] ON [Notifications] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507173638_AddNotificationSystem'
)
BEGIN
    CREATE INDEX [IX_Notifications_Dedup] ON [Notifications] ([RecipientId], [Type], [EntityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507173638_AddNotificationSystem'
)
BEGIN
    CREATE INDEX [IX_Notifications_Recipient_Read_Created] ON [Notifications] ([RecipientId], [IsRead], [CreatedAt] DESC);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507173638_AddNotificationSystem'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260507173638_AddNotificationSystem', N'9.0.10');
END;

COMMIT;
GO

