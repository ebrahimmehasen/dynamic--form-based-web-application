using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRegistry.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentEligibilityStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EligibilityConfirmedAt",
                schema: "dbo",
                table: "Students",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EligibilityConfirmedBy",
                schema: "dbo",
                table: "Students",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EligibilityStatus",
                schema: "dbo",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EligibilityConfirmedAt",
                schema: "dbo",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "EligibilityConfirmedBy",
                schema: "dbo",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "EligibilityStatus",
                schema: "dbo",
                table: "Students");
        }
    }
}
