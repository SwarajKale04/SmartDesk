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
    WHERE [MigrationId] = N'20260824103330_InitialCreate'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824103330_InitialCreate'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Type] nvarchar(100) NOT NULL,
        [Message] nvarchar(1000) NOT NULL,
        [RelatedTicketId] uniqueidentifier NULL,
        [IsRead] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824103330_InitialCreate'
)
BEGIN
    CREATE TABLE [SlaPolicies] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Priority] int NOT NULL,
        [ResponseTimeMinutes] int NOT NULL,
        [ResolutionTimeMinutes] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_SlaPolicies] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824103330_InitialCreate'
)
BEGIN
    CREATE TABLE [TicketComments] (
        [Id] uniqueidentifier NOT NULL,
        [TicketId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [IsInternal] bit NOT NULL,
        CONSTRAINT [PK_TicketComments] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824103330_InitialCreate'
)
BEGIN
    CREATE TABLE [TicketHistories] (
        [Id] uniqueidentifier NOT NULL,
        [TicketId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NULL,
        [Action] nvarchar(100) NOT NULL,
        [OldValue] nvarchar(max) NULL,
        [NewValue] nvarchar(max) NULL,
        [Timestamp] datetimeoffset NOT NULL,
        CONSTRAINT [PK_TicketHistories] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824103330_InitialCreate'
)
BEGIN
    CREATE TABLE [Tickets] (
        [Id] uniqueidentifier NOT NULL,
        [TicketNumber] nvarchar(32) NOT NULL,
        [Title] nvarchar(250) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [CategoryId] uniqueidentifier NULL,
        [Priority] int NOT NULL,
        [Status] int NOT NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [AssignedAgentId] uniqueidentifier NULL,
        [AiConfidence] decimal(5,4) NULL,
        [AiPredictedCategory] nvarchar(max) NULL,
        [AiPredictedPriority] int NULL,
        [AiReviewRequired] bit NOT NULL,
        [SlaId] uniqueidentifier NULL,
        [SlaStatus] int NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NOT NULL,
        [FirstResponseAt] datetimeoffset NULL,
        [DueAt] datetimeoffset NULL,
        [ResolvedAt] datetimeoffset NULL,
        [ClosedAt] datetimeoffset NULL,
        CONSTRAINT [PK_Tickets] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824103330_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Email] nvarchar(320) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Role] int NOT NULL,
        [Department] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824103330_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Categories_Name] ON [Categories] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824103330_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId_IsRead_CreatedAt] ON [Notifications] ([UserId], [IsRead], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824103330_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SlaPolicies_Priority_IsActive] ON [SlaPolicies] ([Priority], [IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824103330_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TicketComments_TicketId_CreatedAt] ON [TicketComments] ([TicketId], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824103330_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TicketHistories_TicketId_Timestamp] ON [TicketHistories] ([TicketId], [Timestamp]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824103330_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tickets_AssignedAgentId] ON [Tickets] ([AssignedAgentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824103330_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tickets_CreatedAt] ON [Tickets] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824103330_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tickets_CustomerId] ON [Tickets] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824103330_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tickets_Status_Priority] ON [Tickets] ([Status], [Priority]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824103330_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Tickets_TicketNumber] ON [Tickets] ([TicketNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824103330_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824103330_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260824103330_InitialCreate', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824105605_AddSlaDeadlines'
)
BEGIN
    ALTER TABLE [Tickets] ADD [FirstResponseDueAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824105605_AddSlaDeadlines'
)
BEGIN
    CREATE INDEX [IX_Tickets_DueAt] ON [Tickets] ([DueAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824105605_AddSlaDeadlines'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260824105605_AddSlaDeadlines', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825181651_AddAiClassificationStatus'
)
BEGIN
    ALTER TABLE [Tickets] ADD [AiClassificationStatus] nvarchar(40) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825181651_AddAiClassificationStatus'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260825181651_AddAiClassificationStatus', N'8.0.19');
END;
GO

COMMIT;
GO

