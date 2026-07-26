using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRegistry.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAmericanDiplomaActSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActComposite",
                schema: "dbo",
                table: "AmericanDiplomaStudentTotals",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActMath",
                schema: "dbo",
                table: "AmericanDiplomaStudentTotals",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestType1",
                schema: "dbo",
                table: "AmericanDiplomaStudentTotals",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TestType2",
                schema: "dbo",
                table: "AmericanDiplomaStudentTotals",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActComposite",
                schema: "dbo",
                table: "AmericanDiplomaStudentTotals");

            migrationBuilder.DropColumn(
                name: "ActMath",
                schema: "dbo",
                table: "AmericanDiplomaStudentTotals");

            migrationBuilder.DropColumn(
                name: "TestType1",
                schema: "dbo",
                table: "AmericanDiplomaStudentTotals");

            migrationBuilder.DropColumn(
                name: "TestType2",
                schema: "dbo",
                table: "AmericanDiplomaStudentTotals");
        }
    }
}
