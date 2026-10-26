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
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261016234054_InitialEmptyCheck', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [AppUsers] (
    [id] int NOT NULL IDENTITY,
    [first_name] nvarchar(100) NOT NULL,
    [last_name] nvarchar(100) NOT NULL,
    [role] nvarchar(20) NOT NULL,
    [address] nvarchar(500) NULL,
    [is_suspended] bit NOT NULL DEFAULT CAST(0 AS bit),
    [is_deleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [points_balance] int NOT NULL DEFAULT 0,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [email] nvarchar(256) NOT NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [phone] nvarchar(30) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AppUsers] PRIMARY KEY ([id])
);

CREATE TABLE [AspNetRoles] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [Categories] (
    [id] int NOT NULL IDENTITY,
    [name] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([id])
);

CREATE TABLE [Cuisines] (
    [id] int NOT NULL IDENTITY,
    [name] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Cuisines] PRIMARY KEY ([id])
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AppUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AppUsers] ([id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AppUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AppUsers] ([id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] int NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AppUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AppUsers] ([id]) ON DELETE CASCADE
);

CREATE TABLE [AssistantConversations] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [PK_AssistantConversations] PRIMARY KEY ([id]),
    CONSTRAINT [FK_AssistantConversations_AppUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AppUsers] ([id]) ON DELETE CASCADE
);

CREATE TABLE [AssistantUsages] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [usage_date] date NOT NULL,
    [request_count] int NOT NULL DEFAULT 0,
    CONSTRAINT [PK_AssistantUsages] PRIMARY KEY ([id]),
    CONSTRAINT [FK_AssistantUsages_AppUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AppUsers] ([id]) ON DELETE CASCADE
);

CREATE TABLE [ChefProfiles] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [kitchen_name] nvarchar(150) NOT NULL,
    [bio] nvarchar(1000) NULL,
    [photo_url] nvarchar(500) NULL,
    [address] nvarchar(300) NULL,
    [status] nvarchar(30) NOT NULL DEFAULT N'Pending',
    [approval_notes] nvarchar(1000) NULL,
    [rejection_reason] nvarchar(1000) NULL,
    [has_accepted_terms] bit NOT NULL DEFAULT CAST(0 AS bit),
    [commission_rate] decimal(5,4) NULL,
    CONSTRAINT [PK_ChefProfiles] PRIMARY KEY ([id]),
    CONSTRAINT [FK_ChefProfiles_AppUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AppUsers] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [Faqs] (
    [id] int NOT NULL IDENTITY,
    [updated_by_user_id] int NULL,
    [question] nvarchar(500) NOT NULL,
    [answer] nvarchar(4000) NOT NULL,
    [updated_at] datetime2 NOT NULL,
    CONSTRAINT [PK_Faqs] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Faqs_AppUsers_updated_by_user_id] FOREIGN KEY ([updated_by_user_id]) REFERENCES [AppUsers] ([id]) ON DELETE SET NULL
);

CREATE TABLE [Notifications] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [type] nvarchar(50) NOT NULL,
    [message] nvarchar(1000) NOT NULL,
    [link] nvarchar(500) NULL,
    [is_read] bit NOT NULL DEFAULT CAST(0 AS bit),
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Notifications_AppUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AppUsers] ([id]) ON DELETE CASCADE
);

CREATE TABLE [PlatformSettings] (
    [id] int NOT NULL IDENTITY,
    [updated_by_user_id] int NULL,
    [commission_rate] decimal(5,4) NOT NULL DEFAULT 0.15,
    [refund_window_hours] int NOT NULL DEFAULT 24,
    [updated_at] datetime2 NOT NULL,
    CONSTRAINT [PK_PlatformSettings] PRIMARY KEY ([id]),
    CONSTRAINT [FK_PlatformSettings_AppUsers_updated_by_user_id] FOREIGN KEY ([updated_by_user_id]) REFERENCES [AppUsers] ([id]) ON DELETE SET NULL
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] int NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] int NOT NULL,
    [RoleId] int NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AppUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AppUsers] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AssistantMessages] (
    [id] int NOT NULL IDENTITY,
    [assistant_conversation_id] int NOT NULL,
    [role] nvarchar(20) NOT NULL,
    [content] nvarchar(max) NOT NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [PK_AssistantMessages] PRIMARY KEY ([id]),
    CONSTRAINT [FK_AssistantMessages_AssistantConversations_assistant_conversation_id] FOREIGN KEY ([assistant_conversation_id]) REFERENCES [AssistantConversations] ([id]) ON DELETE CASCADE
);

CREATE TABLE [Carts] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NULL,
    [chef_profile_id] int NOT NULL,
    [guest_token] nvarchar(100) NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [PK_Carts] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Carts_AppUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AppUsers] ([id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Carts_ChefProfiles_chef_profile_id] FOREIGN KEY ([chef_profile_id]) REFERENCES [ChefProfiles] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [ChefDocuments] (
    [id] int NOT NULL IDENTITY,
    [chef_profile_id] int NOT NULL,
    [doc_type] nvarchar(50) NOT NULL,
    [file_url] nvarchar(500) NOT NULL,
    [uploaded_at] datetime2 NOT NULL,
    CONSTRAINT [PK_ChefDocuments] PRIMARY KEY ([id]),
    CONSTRAINT [FK_ChefDocuments_ChefProfiles_chef_profile_id] FOREIGN KEY ([chef_profile_id]) REFERENCES [ChefProfiles] ([id]) ON DELETE CASCADE
);

CREATE TABLE [ChefWorkingHours] (
    [chef_profile_id] int NOT NULL,
    [day_of_week] tinyint NOT NULL,
    [open_time] time NOT NULL,
    [close_time] time NOT NULL,
    CONSTRAINT [PK_ChefWorkingHours] PRIMARY KEY ([chef_profile_id], [day_of_week]),
    CONSTRAINT [FK_ChefWorkingHours_ChefProfiles_chef_profile_id] FOREIGN KEY ([chef_profile_id]) REFERENCES [ChefProfiles] ([id]) ON DELETE CASCADE
);

CREATE TABLE [Conversations] (
    [id] int NOT NULL IDENTITY,
    [customer_user_id] int NOT NULL,
    [chef_profile_id] int NOT NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [PK_Conversations] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Conversations_AppUsers_customer_user_id] FOREIGN KEY ([customer_user_id]) REFERENCES [AppUsers] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Conversations_ChefProfiles_chef_profile_id] FOREIGN KEY ([chef_profile_id]) REFERENCES [ChefProfiles] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [Dishes] (
    [id] int NOT NULL IDENTITY,
    [chef_profile_id] int NOT NULL,
    [category_id] int NOT NULL,
    [cuisine_id] int NOT NULL,
    [name] nvarchar(150) NOT NULL,
    [description] nvarchar(1000) NULL,
    [price] decimal(10,2) NOT NULL,
    [image_url] nvarchar(500) NULL,
    [is_available] bit NOT NULL DEFAULT CAST(1 AS bit),
    [is_archived] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Dishes] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Dishes_Categories_category_id] FOREIGN KEY ([category_id]) REFERENCES [Categories] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Dishes_ChefProfiles_chef_profile_id] FOREIGN KEY ([chef_profile_id]) REFERENCES [ChefProfiles] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Dishes_Cuisines_cuisine_id] FOREIGN KEY ([cuisine_id]) REFERENCES [Cuisines] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [Favorites] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [chef_profile_id] int NOT NULL,
    CONSTRAINT [PK_Favorites] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Favorites_AppUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AppUsers] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Favorites_ChefProfiles_chef_profile_id] FOREIGN KEY ([chef_profile_id]) REFERENCES [ChefProfiles] ([id]) ON DELETE CASCADE
);

CREATE TABLE [Orders] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [chef_profile_id] int NOT NULL,
    [status] nvarchar(30) NOT NULL DEFAULT N'Pending',
    [payment_method] nvarchar(30) NOT NULL DEFAULT N'Cash',
    [payment_status] nvarchar(30) NOT NULL DEFAULT N'Pending',
    [reference] nvarchar(50) NOT NULL,
    [submission_token] nvarchar(100) NULL,
    [subtotal] decimal(10,2) NOT NULL,
    [points_discount] decimal(10,2) NOT NULL DEFAULT 0.0,
    [total] decimal(10,2) NOT NULL,
    [commission_rate] decimal(5,4) NOT NULL DEFAULT 0.15,
    [commission_amount] decimal(10,2) NOT NULL DEFAULT 0.0,
    [delivery_address] nvarchar(500) NOT NULL,
    [cancellation_reason] nvarchar(500) NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Orders_AppUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AppUsers] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Orders_ChefProfiles_chef_profile_id] FOREIGN KEY ([chef_profile_id]) REFERENCES [ChefProfiles] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [Payouts] (
    [id] int NOT NULL IDENTITY,
    [chef_profile_id] int NOT NULL,
    [amount] decimal(10,2) NOT NULL,
    [status] nvarchar(30) NOT NULL DEFAULT N'Pending',
    [reference] nvarchar(100) NULL,
    [idempotency_key] nvarchar(100) NULL,
    [is_simulated] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Payouts] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Payouts_ChefProfiles_chef_profile_id] FOREIGN KEY ([chef_profile_id]) REFERENCES [ChefProfiles] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [Messages] (
    [id] int NOT NULL IDENTITY,
    [conversation_id] int NOT NULL,
    [sender_user_id] int NOT NULL,
    [text] nvarchar(2000) NOT NULL,
    [sent_at] datetime2 NOT NULL,
    [is_read] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Messages] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Messages_AppUsers_sender_user_id] FOREIGN KEY ([sender_user_id]) REFERENCES [AppUsers] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Messages_Conversations_conversation_id] FOREIGN KEY ([conversation_id]) REFERENCES [Conversations] ([id]) ON DELETE CASCADE
);

CREATE TABLE [CartItems] (
    [id] int NOT NULL IDENTITY,
    [cart_id] int NOT NULL,
    [dish_id] int NOT NULL,
    [quantity] int NOT NULL DEFAULT 1,
    [note] nvarchar(500) NULL,
    CONSTRAINT [PK_CartItems] PRIMARY KEY ([id]),
    CONSTRAINT [FK_CartItems_Carts_cart_id] FOREIGN KEY ([cart_id]) REFERENCES [Carts] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CartItems_Dishes_dish_id] FOREIGN KEY ([dish_id]) REFERENCES [Dishes] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [DishOptions] (
    [id] int NOT NULL IDENTITY,
    [dish_id] int NOT NULL,
    [group_name] nvarchar(100) NOT NULL,
    [name] nvarchar(100) NOT NULL,
    [extra_price] decimal(10,2) NOT NULL DEFAULT 0.0,
    [is_required] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_DishOptions] PRIMARY KEY ([id]),
    CONSTRAINT [FK_DishOptions_Dishes_dish_id] FOREIGN KEY ([dish_id]) REFERENCES [Dishes] ([id]) ON DELETE CASCADE
);

CREATE TABLE [OrderItems] (
    [order_id] int NOT NULL,
    [line_no] int NOT NULL,
    [dish_id] int NOT NULL,
    [dish_name] nvarchar(150) NOT NULL,
    [unit_price] decimal(10,2) NOT NULL,
    [quantity] int NOT NULL DEFAULT 1,
    [note] nvarchar(500) NULL,
    CONSTRAINT [PK_OrderItems] PRIMARY KEY ([order_id], [line_no]),
    CONSTRAINT [FK_OrderItems_Dishes_dish_id] FOREIGN KEY ([dish_id]) REFERENCES [Dishes] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_OrderItems_Orders_order_id] FOREIGN KEY ([order_id]) REFERENCES [Orders] ([id]) ON DELETE CASCADE
);

CREATE TABLE [OrderStatusHistories] (
    [id] int NOT NULL IDENTITY,
    [order_id] int NOT NULL,
    [changed_by_user_id] int NULL,
    [status] nvarchar(30) NOT NULL,
    [changed_at] datetime2 NOT NULL,
    CONSTRAINT [PK_OrderStatusHistories] PRIMARY KEY ([id]),
    CONSTRAINT [FK_OrderStatusHistories_AppUsers_changed_by_user_id] FOREIGN KEY ([changed_by_user_id]) REFERENCES [AppUsers] ([id]) ON DELETE SET NULL,
    CONSTRAINT [FK_OrderStatusHistories_Orders_order_id] FOREIGN KEY ([order_id]) REFERENCES [Orders] ([id]) ON DELETE CASCADE
);

CREATE TABLE [Payments] (
    [id] int NOT NULL IDENTITY,
    [order_id] int NOT NULL,
    [amount] decimal(10,2) NOT NULL,
    [status] nvarchar(30) NOT NULL DEFAULT N'Pending',
    [stripe_reference] nvarchar(100) NULL,
    [unsettled_amount] decimal(10,2) NOT NULL DEFAULT 0.0,
    CONSTRAINT [PK_Payments] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Payments_Orders_order_id] FOREIGN KEY ([order_id]) REFERENCES [Orders] ([id]) ON DELETE CASCADE
);

CREATE TABLE [PointsLedgers] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [order_id] int NULL,
    [points] int NOT NULL,
    [type] nvarchar(30) NOT NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [PK_PointsLedgers] PRIMARY KEY ([id]),
    CONSTRAINT [FK_PointsLedgers_AppUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AppUsers] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PointsLedgers_Orders_order_id] FOREIGN KEY ([order_id]) REFERENCES [Orders] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [RefundRequests] (
    [id] int NOT NULL IDENTITY,
    [order_id] int NOT NULL,
    [user_id] int NOT NULL,
    [reason] nvarchar(1000) NOT NULL,
    [status] nvarchar(30) NOT NULL DEFAULT N'Requested',
    [amount] decimal(10,2) NOT NULL,
    [requested_at] datetime2 NOT NULL,
    [decision] nvarchar(30) NULL,
    [decision_reason] nvarchar(1000) NULL,
    [decided_at] datetime2 NULL,
    [stripe_refund_ref] nvarchar(100) NULL,
    CONSTRAINT [PK_RefundRequests] PRIMARY KEY ([id]),
    CONSTRAINT [FK_RefundRequests_AppUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AppUsers] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RefundRequests_Orders_order_id] FOREIGN KEY ([order_id]) REFERENCES [Orders] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [Reviews] (
    [id] int NOT NULL IDENTITY,
    [order_id] int NOT NULL,
    [user_id] int NOT NULL,
    [chef_profile_id] int NOT NULL,
    [rating] tinyint NOT NULL,
    [comment] nvarchar(2000) NULL,
    [moderation_status] nvarchar(30) NOT NULL DEFAULT N'Pending',
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [PK_Reviews] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Reviews_AppUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AppUsers] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Reviews_ChefProfiles_chef_profile_id] FOREIGN KEY ([chef_profile_id]) REFERENCES [ChefProfiles] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Reviews_Orders_order_id] FOREIGN KEY ([order_id]) REFERENCES [Orders] ([id]) ON DELETE CASCADE
);

CREATE TABLE [SupportCases] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [order_id] int NULL,
    [subject] nvarchar(200) NOT NULL,
    [description] nvarchar(4000) NOT NULL,
    [status] nvarchar(30) NOT NULL DEFAULT N'Open',
    [requester_role] nvarchar(20) NOT NULL,
    [awaiting_support] bit NOT NULL DEFAULT CAST(1 AS bit),
    [created_at] datetime2 NOT NULL,
    [last_activity_at] datetime2 NULL,
    [closed_at] datetime2 NULL,
    CONSTRAINT [PK_SupportCases] PRIMARY KEY ([id]),
    CONSTRAINT [FK_SupportCases_AppUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AppUsers] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SupportCases_Orders_order_id] FOREIGN KEY ([order_id]) REFERENCES [Orders] ([id]) ON DELETE SET NULL
);

CREATE TABLE [CartItemOptions] (
    [id] int NOT NULL IDENTITY,
    [cart_item_id] int NOT NULL,
    [dish_option_id] int NOT NULL,
    CONSTRAINT [PK_CartItemOptions] PRIMARY KEY ([id]),
    CONSTRAINT [FK_CartItemOptions_CartItems_cart_item_id] FOREIGN KEY ([cart_item_id]) REFERENCES [CartItems] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CartItemOptions_DishOptions_dish_option_id] FOREIGN KEY ([dish_option_id]) REFERENCES [DishOptions] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [OrderItemOptions] (
    [id] int NOT NULL IDENTITY,
    [order_id] int NOT NULL,
    [order_item_line_no] int NOT NULL,
    [group_name] nvarchar(100) NOT NULL,
    [option_name] nvarchar(100) NOT NULL,
    [extra_price] decimal(10,2) NOT NULL DEFAULT 0.0,
    CONSTRAINT [PK_OrderItemOptions] PRIMARY KEY ([id]),
    CONSTRAINT [FK_OrderItemOptions_OrderItems_order_id_order_item_line_no] FOREIGN KEY ([order_id], [order_item_line_no]) REFERENCES [OrderItems] ([order_id], [line_no]) ON DELETE CASCADE
);

CREATE TABLE [LedgerEntries] (
    [id] int NOT NULL IDENTITY,
    [chef_profile_id] int NOT NULL,
    [order_id] int NULL,
    [payout_id] int NULL,
    [refund_request_id] int NULL,
    [type] nvarchar(30) NOT NULL,
    [amount] decimal(10,2) NOT NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [PK_LedgerEntries] PRIMARY KEY ([id]),
    CONSTRAINT [FK_LedgerEntries_ChefProfiles_chef_profile_id] FOREIGN KEY ([chef_profile_id]) REFERENCES [ChefProfiles] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_LedgerEntries_Orders_order_id] FOREIGN KEY ([order_id]) REFERENCES [Orders] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_LedgerEntries_Payouts_payout_id] FOREIGN KEY ([payout_id]) REFERENCES [Payouts] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_LedgerEntries_RefundRequests_refund_request_id] FOREIGN KEY ([refund_request_id]) REFERENCES [RefundRequests] ([id]) ON DELETE NO ACTION
);

CREATE TABLE [CaseMessages] (
    [id] int NOT NULL IDENTITY,
    [support_case_id] int NOT NULL,
    [author_user_id] int NOT NULL,
    [author_role] nvarchar(20) NOT NULL,
    [text] nvarchar(4000) NOT NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [PK_CaseMessages] PRIMARY KEY ([id]),
    CONSTRAINT [FK_CaseMessages_AppUsers_author_user_id] FOREIGN KEY ([author_user_id]) REFERENCES [AppUsers] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CaseMessages_SupportCases_support_case_id] FOREIGN KEY ([support_case_id]) REFERENCES [SupportCases] ([id]) ON DELETE CASCADE
);

CREATE INDEX [EmailIndex] ON [AppUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [IX_AppUsers_email] ON [AppUsers] ([email]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AppUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [IX_AssistantConversations_user_id] ON [AssistantConversations] ([user_id]);

CREATE INDEX [IX_AssistantMessages_assistant_conversation_id] ON [AssistantMessages] ([assistant_conversation_id]);

CREATE UNIQUE INDEX [IX_AssistantUsages_user_id_usage_date] ON [AssistantUsages] ([user_id], [usage_date]);

CREATE INDEX [IX_CartItemOptions_cart_item_id] ON [CartItemOptions] ([cart_item_id]);

CREATE INDEX [IX_CartItemOptions_dish_option_id] ON [CartItemOptions] ([dish_option_id]);

CREATE INDEX [IX_CartItems_cart_id] ON [CartItems] ([cart_id]);

CREATE INDEX [IX_CartItems_dish_id] ON [CartItems] ([dish_id]);

CREATE INDEX [IX_Carts_chef_profile_id] ON [Carts] ([chef_profile_id]);

CREATE INDEX [IX_Carts_user_id] ON [Carts] ([user_id]);

CREATE INDEX [IX_CaseMessages_author_user_id] ON [CaseMessages] ([author_user_id]);

CREATE INDEX [IX_CaseMessages_support_case_id] ON [CaseMessages] ([support_case_id]);

CREATE INDEX [IX_ChefDocuments_chef_profile_id] ON [ChefDocuments] ([chef_profile_id]);

CREATE UNIQUE INDEX [IX_ChefProfiles_user_id] ON [ChefProfiles] ([user_id]);

CREATE INDEX [IX_Conversations_chef_profile_id] ON [Conversations] ([chef_profile_id]);

CREATE UNIQUE INDEX [IX_Conversations_customer_user_id_chef_profile_id] ON [Conversations] ([customer_user_id], [chef_profile_id]);

CREATE INDEX [IX_Dishes_category_id] ON [Dishes] ([category_id]);

CREATE INDEX [IX_Dishes_chef_profile_id] ON [Dishes] ([chef_profile_id]);

CREATE INDEX [IX_Dishes_cuisine_id] ON [Dishes] ([cuisine_id]);

CREATE INDEX [IX_DishOptions_dish_id] ON [DishOptions] ([dish_id]);

CREATE INDEX [IX_Faqs_updated_by_user_id] ON [Faqs] ([updated_by_user_id]);

CREATE INDEX [IX_Favorites_chef_profile_id] ON [Favorites] ([chef_profile_id]);

CREATE UNIQUE INDEX [IX_Favorites_user_id_chef_profile_id] ON [Favorites] ([user_id], [chef_profile_id]);

CREATE INDEX [IX_LedgerEntries_chef_profile_id] ON [LedgerEntries] ([chef_profile_id]);

CREATE INDEX [IX_LedgerEntries_order_id] ON [LedgerEntries] ([order_id]);

CREATE INDEX [IX_LedgerEntries_payout_id] ON [LedgerEntries] ([payout_id]);

CREATE INDEX [IX_LedgerEntries_refund_request_id] ON [LedgerEntries] ([refund_request_id]);

CREATE INDEX [IX_Messages_conversation_id] ON [Messages] ([conversation_id]);

CREATE INDEX [IX_Messages_sender_user_id] ON [Messages] ([sender_user_id]);

CREATE INDEX [IX_Notifications_user_id] ON [Notifications] ([user_id]);

CREATE INDEX [IX_OrderItemOptions_order_id_order_item_line_no] ON [OrderItemOptions] ([order_id], [order_item_line_no]);

CREATE INDEX [IX_OrderItems_dish_id] ON [OrderItems] ([dish_id]);

CREATE INDEX [IX_Orders_chef_profile_id] ON [Orders] ([chef_profile_id]);

CREATE UNIQUE INDEX [IX_Orders_reference] ON [Orders] ([reference]);

CREATE UNIQUE INDEX [IX_Orders_submission_token] ON [Orders] ([submission_token]) WHERE [submission_token] IS NOT NULL;

CREATE INDEX [IX_Orders_user_id] ON [Orders] ([user_id]);

CREATE INDEX [IX_OrderStatusHistories_changed_by_user_id] ON [OrderStatusHistories] ([changed_by_user_id]);

CREATE INDEX [IX_OrderStatusHistories_order_id] ON [OrderStatusHistories] ([order_id]);

CREATE UNIQUE INDEX [IX_Payments_order_id] ON [Payments] ([order_id]);

CREATE INDEX [IX_Payouts_chef_profile_id] ON [Payouts] ([chef_profile_id]);

CREATE UNIQUE INDEX [IX_Payouts_idempotency_key] ON [Payouts] ([idempotency_key]) WHERE [idempotency_key] IS NOT NULL;

CREATE INDEX [IX_PlatformSettings_updated_by_user_id] ON [PlatformSettings] ([updated_by_user_id]);

CREATE INDEX [IX_PointsLedgers_order_id] ON [PointsLedgers] ([order_id]);

CREATE INDEX [IX_PointsLedgers_user_id] ON [PointsLedgers] ([user_id]);

CREATE INDEX [IX_RefundRequests_order_id] ON [RefundRequests] ([order_id]);

CREATE INDEX [IX_RefundRequests_user_id] ON [RefundRequests] ([user_id]);

CREATE INDEX [IX_Reviews_chef_profile_id] ON [Reviews] ([chef_profile_id]);

CREATE UNIQUE INDEX [IX_Reviews_order_id] ON [Reviews] ([order_id]);

CREATE INDEX [IX_Reviews_user_id] ON [Reviews] ([user_id]);

CREATE INDEX [IX_SupportCases_order_id] ON [SupportCases] ([order_id]);

CREATE INDEX [IX_SupportCases_user_id] ON [SupportCases] ([user_id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261026010829_InitialDatabaseSchema', N'10.0.12');

COMMIT;
GO

