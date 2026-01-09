using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASTRASystem.Migrations
{
    /// <inheritdoc />
    public partial class IsActiveWarehoouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Warehouses",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Warehouses");
        }
    }
}
