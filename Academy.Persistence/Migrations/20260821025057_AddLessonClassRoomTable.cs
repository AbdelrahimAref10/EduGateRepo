using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Academy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonClassRoomTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "LessonGroupSessions",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Topic",
                table: "LessonGroupSessions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LessonSessionMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LessonGroupSessionId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MaterialType = table.Column<int>(type: "int", nullable: false),
                    ExternalUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StoredFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", maxLength: 20000, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonSessionMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonSessionMaterials_LessonGroupSessions_LessonGroupSessionId",
                        column: x => x.LessonGroupSessionId,
                        principalTable: "LessonGroupSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonSessionMaterials_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LessonSessionStudentDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LessonGroupSessionId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    IsPresent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TeacherNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonSessionStudentDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonSessionStudentDetails_LessonGroupSessions_LessonGroupSessionId",
                        column: x => x.LessonGroupSessionId,
                        principalTable: "LessonGroupSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonSessionStudentDetails_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonSessionMaterials_CreatedByUserId",
                table: "LessonSessionMaterials",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonSessionMaterials_LessonGroupSessionId",
                table: "LessonSessionMaterials",
                column: "LessonGroupSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonSessionMaterials_LessonGroupSessionId_SortOrder",
                table: "LessonSessionMaterials",
                columns: new[] { "LessonGroupSessionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_LessonSessionStudentDetails_LessonGroupSessionId_StudentId",
                table: "LessonSessionStudentDetails",
                columns: new[] { "LessonGroupSessionId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonSessionStudentDetails_StudentId",
                table: "LessonSessionStudentDetails",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonSessionMaterials");

            migrationBuilder.DropTable(
                name: "LessonSessionStudentDetails");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "LessonGroupSessions");

            migrationBuilder.DropColumn(
                name: "Topic",
                table: "LessonGroupSessions");
        }
    }
}
