using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Academy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherListIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Lessons_TeacherId_CreatedAtUtc",
                table: "Lessons",
                columns: new[] { "TeacherId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_TeacherId_EducationTypeId",
                table: "Lessons",
                columns: new[] { "TeacherId", "EducationTypeId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lessons_TeacherId_CreatedAtUtc",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_TeacherId_EducationTypeId",
                table: "Lessons");
        }
    }
}
