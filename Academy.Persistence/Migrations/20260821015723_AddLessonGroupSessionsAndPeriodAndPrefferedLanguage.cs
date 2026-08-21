using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Academy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonGroupSessionsAndPeriodAndPrefferedLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PreferredLanguage",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndedAtUtc",
                table: "LessonGroups",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PeriodEndDate",
                table: "LessonGroups",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "PeriodStartDate",
                table: "LessonGroups",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateTable(
                name: "LessonGroupSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LessonGroupId = table.Column<int>(type: "int", nullable: false),
                    SessionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonGroupSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonGroupSessions_LessonGroups_LessonGroupId",
                        column: x => x.LessonGroupId,
                        principalTable: "LessonGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonGroupSessions_LessonGroupId",
                table: "LessonGroupSessions",
                column: "LessonGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonGroupSessions_LessonGroupId_SessionDate_StartTime",
                table: "LessonGroupSessions",
                columns: new[] { "LessonGroupId", "SessionDate", "StartTime" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonGroupSessions");

            migrationBuilder.DropColumn(
                name: "PreferredLanguage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EndedAtUtc",
                table: "LessonGroups");

            migrationBuilder.DropColumn(
                name: "PeriodEndDate",
                table: "LessonGroups");

            migrationBuilder.DropColumn(
                name: "PeriodStartDate",
                table: "LessonGroups");
        }
    }
}
