using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace depiR5gp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAppUserRoleAndStatus : Migration
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
                """);

            migrationBuilder.Sql(
                """
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
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('AppUsers', 'role') IS NOT NULL
                BEGIN
                    DECLARE @var0 sysname;
                    SELECT @var0 = [d].[name]
                    FROM [sys].[default_constraints] [d]
                    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AppUsers]') AND [c].[name] = N'role');
                    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [AppUsers] DROP CONSTRAINT [' + @var0 + '];');
                    ALTER TABLE [AppUsers] DROP COLUMN [role];
                END
                """);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "AppUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AppUsers_status",
                table: "AppUsers",
                sql: "status IN ('Active', 'Suspended', 'Deleted')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<string>(
                name: "role",
                table: "AppUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
