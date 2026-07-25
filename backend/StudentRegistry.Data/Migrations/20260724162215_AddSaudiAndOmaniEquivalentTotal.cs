using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRegistry.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSaudiAndOmaniEquivalentTotal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EquivalentTotal",
                schema: "dbo",
                table: "SaudiStudentTotals",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EquivalentTotal",
                schema: "dbo",
                table: "OmaniStudentTotals",
                type: "decimal(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EquivalentTotal",
                schema: "dbo",
                table: "SaudiStudentTotals");

            migrationBuilder.DropColumn(
                name: "EquivalentTotal",
                schema: "dbo",
                table: "OmaniStudentTotals");
        }
    }
}
