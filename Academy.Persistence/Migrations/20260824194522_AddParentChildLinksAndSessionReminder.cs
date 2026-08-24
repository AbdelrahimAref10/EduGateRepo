using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Academy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddParentChildLinksAndSessionReminder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StartingSoonReminderSentAtUtc",
                table: "LessonGroupSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ParentChildLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentStudentId = table.Column<int>(type: "int", nullable: false),
                    ChildStudentId = table.Column<int>(type: "int", nullable: false),
                    LinkedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentChildLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParentChildLinks_Students_ChildStudentId",
                        column: x => x.ChildStudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParentChildLinks_Students_ParentStudentId",
                        column: x => x.ParentStudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParentChildLinks_ChildStudentId",
                table: "ParentChildLinks",
                column: "ChildStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentChildLinks_ParentStudentId_ChildStudentId",
                table: "ParentChildLinks",
                columns: new[] { "ParentStudentId", "ChildStudentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParentChildLinks");

            migrationBuilder.DropColumn(
                name: "StartingSoonReminderSentAtUtc",
                table: "LessonGroupSessions");
        }
    }
}
