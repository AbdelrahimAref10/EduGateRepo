using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Academy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonAreaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AreaId",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_AreaId",
                table: "Lessons",
                column: "AreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Areas_AreaId",
                table: "Lessons",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Areas_AreaId",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_AreaId",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "Lessons");
        }
    }
}
