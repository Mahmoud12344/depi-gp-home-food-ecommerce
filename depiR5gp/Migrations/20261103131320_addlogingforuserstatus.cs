using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace depiR5gp.Migrations
{
    /// <inheritdoc />
    public partial class addlogingforuserstatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('AppUsers', 'is_deleted') IS NOT NULL
                BEGIN
                    DECLARE @var_del sysname;
                    SELECT @var_del = [d].[name]
                    FROM [sys].[default_constraints] [d]
                    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AppUsers]') AND [c].[name] = N'is_deleted');
                    IF @var_del IS NOT NULL EXEC(N'ALTER TABLE [AppUsers] DROP CONSTRAINT [' + @var_del + '];');
                    ALTER TABLE [AppUsers] DROP COLUMN [is_deleted];
                END

                IF COL_LENGTH('AppUsers', 'is_suspended') IS NOT NULL
                BEGIN
                    DECLARE @var_susp sysname;
                    SELECT @var_susp = [d].[name]
                    FROM [sys].[default_constraints] [d]
                    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AppUsers]') AND [c].[name] = N'is_suspended');
                    IF @var_susp IS NOT NULL EXEC(N'ALTER TABLE [AppUsers] DROP CONSTRAINT [' + @var_susp + '];');
                    ALTER TABLE [AppUsers] DROP COLUMN [is_suspended];
                END

                IF COL_LENGTH('AppUsers', 'status') IS NULL
                BEGIN
                    ALTER TABLE [AppUsers] ADD [status] nvarchar(20) NOT NULL DEFAULT 'Active';
                END

                IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_AppUsers_status')
                BEGIN
                    ALTER TABLE [AppUsers] ADD CONSTRAINT [CK_AppUsers_status] CHECK (status IN ('Active', 'Suspended', 'Deleted'));
                END

                IF OBJECT_ID(N'[UserStatusHistories]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [UserStatusHistories] (
                        [id] int NOT NULL IDENTITY,
                        [user_id] int NOT NULL,
                        [changed_by_user_id] int NULL,
                        [status] nvarchar(20) NOT NULL,
                        [changed_at] datetime2 NOT NULL,
                        [reason] nvarchar(200) NULL,
                        CONSTRAINT [PK_UserStatusHistories] PRIMARY KEY ([id]),
                        CONSTRAINT [CK_UserStatusHistories_status] CHECK (status IN ('Active', 'Suspended', 'Deleted')),
                        CONSTRAINT [FK_UserStatusHistories_AppUsers_changed_by_user_id] FOREIGN KEY ([changed_by_user_id]) REFERENCES [AppUsers] ([id]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_UserStatusHistories_AppUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AppUsers] ([id]) ON DELETE NO ACTION
                    );
                END

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserStatusHistories_changed_by_user_id' AND object_id = OBJECT_ID(N'[UserStatusHistories]'))
                BEGIN
                    CREATE INDEX [IX_UserStatusHistories_changed_by_user_id] ON [UserStatusHistories] ([changed_by_user_id]);
                END

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserStatusHistories_user_changed' AND object_id = OBJECT_ID(N'[UserStatusHistories]'))
                BEGIN
                    CREATE INDEX [IX_UserStatusHistories_user_changed] ON [UserStatusHistories] ([user_id] ASC, [changed_at] DESC);
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserStatusHistories");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AppUsers_status",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "status",
                table: "AppUsers");

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "AppUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_suspended",
                table: "AppUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
