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
            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "is_suspended",
                table: "AppUsers");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "AppUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.CreateTable(
                name: "UserStatusHistories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    changed_by_user_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    changed_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStatusHistories", x => x.id);
                    table.CheckConstraint("CK_UserStatusHistories_status", "status IN ('Active', 'Suspended', 'Deleted')");
                    table.ForeignKey(
                        name: "FK_UserStatusHistories_AppUsers_changed_by_user_id",
                        column: x => x.changed_by_user_id,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserStatusHistories_AppUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AppUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_AppUsers_status",
                table: "AppUsers",
                sql: "status IN ('Active', 'Suspended', 'Deleted')");

            migrationBuilder.CreateIndex(
                name: "IX_UserStatusHistories_changed_by_user_id",
                table: "UserStatusHistories",
                column: "changed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserStatusHistories_user_changed",
                table: "UserStatusHistories",
                columns: new[] { "user_id", "changed_at" },
                descending: new[] { false, true });
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
