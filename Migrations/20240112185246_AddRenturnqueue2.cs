using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projektstudia2.Migrations
{
    /// <inheritdoc />
    public partial class AddRenturnqueue2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BookTitle",
                table: "ReturnQueues",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookTitle",
                table: "ReturnQueues");
        }
    }
}
