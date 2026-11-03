using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace depiR5gp.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAcceptingOrdersToChefProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAcceptingOrders",
                table: "ChefProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAcceptingOrders",
                table: "ChefProfiles");
        }
    }
}
