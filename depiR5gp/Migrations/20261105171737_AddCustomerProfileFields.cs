using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace depiR5gp.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "allergies",
                table: "AppUsers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dietary_preferences",
                table: "AppUsers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "primary_area",
                table: "AppUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "profile_image_path",
                table: "AppUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "allergies",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "dietary_preferences",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "primary_area",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "profile_image_path",
                table: "AppUsers");
        }
    }
}
