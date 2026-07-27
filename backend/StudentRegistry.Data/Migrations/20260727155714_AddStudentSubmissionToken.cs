using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRegistry.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentSubmissionToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubmissionToken",
                schema: "dbo",
                table: "Students",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_SubmissionToken",
                schema: "dbo",
                table: "Students",
                column: "SubmissionToken",
                unique: true,
                filter: "[SubmissionToken] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_SubmissionToken",
                schema: "dbo",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SubmissionToken",
                schema: "dbo",
                table: "Students");
        }
    }
}
