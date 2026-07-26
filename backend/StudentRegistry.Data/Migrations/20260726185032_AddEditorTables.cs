using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRegistry.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEditorTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeleteRequests",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: true),
                    RequestedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "Editor"),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    ReviewedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeleteRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeleteRequests_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "dbo",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FieldComments",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FieldSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommentText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Author = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "Editor"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "unreviewed")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldComments_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "dbo",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FieldEdits",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Editor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "Editor"),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "manual"),
                    SourceCommentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldEdits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldEdits_FieldComments_SourceCommentId",
                        column: x => x.SourceCommentId,
                        principalSchema: "dbo",
                        principalTable: "FieldComments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FieldEdits_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "dbo",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeleteRequests_Status",
                schema: "dbo",
                table: "DeleteRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DeleteRequests_StudentId",
                schema: "dbo",
                table: "DeleteRequests",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldComments_Status",
                schema: "dbo",
                table: "FieldComments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FieldComments_StudentId_FieldName",
                schema: "dbo",
                table: "FieldComments",
                columns: new[] { "StudentId", "FieldName" });

            migrationBuilder.CreateIndex(
                name: "IX_FieldEdits_SourceCommentId",
                schema: "dbo",
                table: "FieldEdits",
                column: "SourceCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldEdits_StudentId_FieldName",
                schema: "dbo",
                table: "FieldEdits",
                columns: new[] { "StudentId", "FieldName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeleteRequests",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "FieldEdits",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "FieldComments",
                schema: "dbo");
        }
    }
}
