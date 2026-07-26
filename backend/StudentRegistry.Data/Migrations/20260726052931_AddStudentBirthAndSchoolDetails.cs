using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRegistry.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentBirthAndSchoolDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BirthCity",
                schema: "dbo",
                table: "Students",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BirthCountry",
                schema: "dbo",
                table: "Students",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                schema: "dbo",
                table: "Students",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "BirthGovernorate",
                schema: "dbo",
                table: "Students",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SchoolName",
                schema: "dbo",
                table: "Students",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthCity",
                schema: "dbo",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "BirthCountry",
                schema: "dbo",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                schema: "dbo",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "BirthGovernorate",
                schema: "dbo",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SchoolName",
                schema: "dbo",
                table: "Students");
        }
    }
}
