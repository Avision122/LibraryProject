using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projektstudia2.Migrations
{
    /// <inheritdoc />
    public partial class ModifyRental : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Rentals");

            migrationBuilder.AddColumn<string>(
                name: "BookTitle",
                table: "Rentals",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UserLogin",
                table: "Rentals",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookTitle",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "UserLogin",
                table: "Rentals");

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Rentals",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
