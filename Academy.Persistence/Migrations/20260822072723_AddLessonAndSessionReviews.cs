using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Academy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonAndSessionReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RatingAverage",
                table: "Lessons",
                type: "decimal(4,2)",
                precision: 4,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "RatingCount",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "RatingAverage",
                table: "LessonGroupSessions",
                type: "decimal(4,2)",
                precision: 4,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "RatingCount",
                table: "LessonGroupSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LessonReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonReviews_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonReviews_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonReviews_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LessonGroupSessionId = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionReviews_LessonGroupSessions_LessonGroupSessionId",
                        column: x => x.LessonGroupSessionId,
                        principalTable: "LessonGroupSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionReviews_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionReviews_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonReviews_LessonId_StudentId",
                table: "LessonReviews",
                columns: new[] { "LessonId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonReviews_StudentId",
                table: "LessonReviews",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonReviews_TeacherId_CreatedAtUtc",
                table: "LessonReviews",
                columns: new[] { "TeacherId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SessionReviews_LessonGroupSessionId_StudentId",
                table: "SessionReviews",
                columns: new[] { "LessonGroupSessionId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionReviews_StudentId",
                table: "SessionReviews",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionReviews_TeacherId_CreatedAtUtc",
                table: "SessionReviews",
                columns: new[] { "TeacherId", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonReviews");

            migrationBuilder.DropTable(
                name: "SessionReviews");

            migrationBuilder.DropColumn(
                name: "RatingAverage",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "RatingCount",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "RatingAverage",
                table: "LessonGroupSessions");

            migrationBuilder.DropColumn(
                name: "RatingCount",
                table: "LessonGroupSessions");
        }
    }
}
