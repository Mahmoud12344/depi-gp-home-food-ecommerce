using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace depiR5gp.Migrations
{
    /// <inheritdoc />
    public partial class AddCartItemPresetKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "preset_key",
                table: "CartItems",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "preset_key",
                table: "CartItems");
        }
    }
}
