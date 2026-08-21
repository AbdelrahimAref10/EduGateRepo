using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Academy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonsGroupTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAtUtc",
                table: "Lessons",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LessonGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AreaId = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MaxCapacity = table.Column<int>(type: "int", nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonGroups_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonGroups_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonGroupDates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LessonGroupId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonGroupDates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonGroupDates_LessonGroups_LessonGroupId",
                        column: x => x.LessonGroupId,
                        principalTable: "LessonGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonGroupMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LessonGroupId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    AddedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonGroupMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonGroupMembers_LessonGroups_LessonGroupId",
                        column: x => x.LessonGroupId,
                        principalTable: "LessonGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonGroupMembers_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonGroupDates_LessonGroupId",
                table: "LessonGroupDates",
                column: "LessonGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonGroupDates_LessonGroupId_DayOfWeek",
                table: "LessonGroupDates",
                columns: new[] { "LessonGroupId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonGroupMembers_LessonGroupId_StudentId",
                table: "LessonGroupMembers",
                columns: new[] { "LessonGroupId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonGroupMembers_StudentId",
                table: "LessonGroupMembers",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonGroups_AreaId",
                table: "LessonGroups",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonGroups_LessonId",
                table: "LessonGroups",
                column: "LessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonGroupDates");

            migrationBuilder.DropTable(
                name: "LessonGroupMembers");

            migrationBuilder.DropTable(
                name: "LessonGroups");

            migrationBuilder.DropColumn(
                name: "StartedAtUtc",
                table: "Lessons");
        }
    }
}
