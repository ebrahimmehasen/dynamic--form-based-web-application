using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRegistry.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificationDisplaySettingsAndUserNationalId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NationalId",
                schema: "dbo",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CertificationDisplaySettings",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CertificationKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsResultVisible = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUsername = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificationDisplaySettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CertificationDisplaySettings_CertificationKey",
                schema: "dbo",
                table: "CertificationDisplaySettings",
                column: "CertificationKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CertificationDisplaySettings",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "NationalId",
                schema: "dbo",
                table: "Users");
        }
    }
}
