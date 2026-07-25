using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRegistry.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAmericanDiplomaCertificateSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AmericanDiplomaStudentTotals",
                schema: "dbo",
                columns: table => new
                {
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    AverageScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    BasePercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SatI = table.Column<int>(type: "int", nullable: false),
                    SatII = table.Column<int>(type: "int", nullable: true),
                    SatIISubject1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SatIISubject2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SatIBelowMinimum = table.Column<bool>(type: "bit", nullable: false),
                    SatIIBelowMinimum = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmericanDiplomaStudentTotals", x => x.StudentId);
                    table.ForeignKey(
                        name: "FK_AmericanDiplomaStudentTotals_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "dbo",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AmericanDiplomaStudentTotals",
                schema: "dbo");
        }
    }
}
