using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace depiR5gp.Migrations
{
    /// <inheritdoc />
    public partial class AddDishFavoritesSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Favorites_user_id_chef_profile_id",
                table: "Favorites");

            migrationBuilder.AlterColumn<int>(
                name: "chef_profile_id",
                table: "Favorites",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "dish_id",
                table: "Favorites",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_dish_id",
                table: "Favorites",
                column: "dish_id");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_user_id_chef_profile_id",
                table: "Favorites",
                columns: new[] { "user_id", "chef_profile_id" },
                unique: true,
                filter: "[chef_profile_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_user_id_dish_id",
                table: "Favorites",
                columns: new[] { "user_id", "dish_id" },
                unique: true,
                filter: "[dish_id] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_Dishes_dish_id",
                table: "Favorites",
                column: "dish_id",
                principalTable: "Dishes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_Dishes_dish_id",
                table: "Favorites");

            migrationBuilder.DropIndex(
                name: "IX_Favorites_dish_id",
                table: "Favorites");

            migrationBuilder.DropIndex(
                name: "IX_Favorites_user_id_chef_profile_id",
                table: "Favorites");

            migrationBuilder.DropIndex(
                name: "IX_Favorites_user_id_dish_id",
                table: "Favorites");

            migrationBuilder.DropColumn(
                name: "dish_id",
                table: "Favorites");

            migrationBuilder.AlterColumn<int>(
                name: "chef_profile_id",
                table: "Favorites",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_user_id_chef_profile_id",
                table: "Favorites",
                columns: new[] { "user_id", "chef_profile_id" },
                unique: true);
        }
    }
}
