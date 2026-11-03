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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
