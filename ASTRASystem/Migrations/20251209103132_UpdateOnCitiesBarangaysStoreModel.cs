using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASTRASystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOnCitiesBarangaysStoreModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stores_Barangay_City",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "Barangay",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Stores");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Stores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerName",
                table: "Stores",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<long>(
                name: "BarangayId",
                table: "Stores",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CityId",
                table: "Stores",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Province = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Region = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Barangays",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CityId = table.Column<long>(type: "bigint", nullable: false),
                    ZipCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Barangays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Barangays_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stores_BarangayId",
                table: "Stores",
                column: "BarangayId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_CityId",
                table: "Stores",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Barangays_CityId_Name",
                table: "Barangays",
                columns: new[] { "CityId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_Name",
                table: "Cities",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_Barangays_BarangayId",
                table: "Stores",
                column: "BarangayId",
                principalTable: "Barangays",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_Cities_CityId",
                table: "Stores",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stores_Barangays_BarangayId",
                table: "Stores");

            migrationBuilder.DropForeignKey(
                name: "FK_Stores_Cities_CityId",
                table: "Stores");

            migrationBuilder.DropTable(
                name: "Barangays");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropIndex(
                name: "IX_Stores_BarangayId",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Stores_CityId",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "BarangayId",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Stores");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Stores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerName",
                table: "Stores",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Barangay",
                table: "Stores",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Stores",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_Barangay_City",
                table: "Stores",
                columns: new[] { "Barangay", "City" });
        }
    }
}
