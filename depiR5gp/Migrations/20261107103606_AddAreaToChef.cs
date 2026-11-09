using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace depiR5gp.Migrations
{
    /// <inheritdoc />
    public partial class AddAreaToChef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "area",
                table: "ChefProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChefProfiles_area",
                table: "ChefProfiles",
                column: "area");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChefProfiles_area",
                table: "ChefProfiles");

            migrationBuilder.DropColumn(
                name: "area",
                table: "ChefProfiles");
        }
    }
}
