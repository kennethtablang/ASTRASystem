using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASTRASystem.Migrations
{
    /// <inheritdoc />
    public partial class PaymentReconciliationAttrib : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Reference",
                table: "Payments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<bool>(
                name: "IsReconciled",
                table: "Payments",
                type: "bit",
                nullable: false,
                defaultValue: false); 

            migrationBuilder.AddColumn<DateTime>(
                name: "ReconciledAt",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReconciledById",
                table: "Payments",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReconciliationNotes",
                table: "Payments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReconciled",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReconciledAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReconciledById",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReconciliationNotes",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "Reference",
                table: "Payments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
