using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRegistry.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAmericanDiplomaStudiedAdvancedMath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "StudiedAdvancedMath",
                schema: "dbo",
                table: "AmericanDiplomaStudentTotals",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StudiedAdvancedMath",
                schema: "dbo",
                table: "AmericanDiplomaStudentTotals");
        }
    }
}
