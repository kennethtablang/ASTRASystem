using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASTRASystem.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationFieldsToStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                table: "Stores",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                table: "Stores",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Stores",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Stores",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressLine1",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Stores");
        }
    }
}
