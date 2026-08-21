using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Academy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccademicYearsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcademicYear",
                table: "Lessons");

            migrationBuilder.AddColumn<int>(
                name: "EducationTypeId",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EducationYearId",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EducationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EducationYears",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EducationTypeId = table.Column<int>(type: "int", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationYears", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EducationYears_EducationTypes_EducationTypeId",
                        column: x => x.EducationTypeId,
                        principalTable: "EducationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_EducationTypeId",
                table: "Lessons",
                column: "EducationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_EducationYearId",
                table: "Lessons",
                column: "EducationYearId");

            migrationBuilder.CreateIndex(
                name: "IX_EducationTypes_NameEn",
                table: "EducationTypes",
                column: "NameEn");

            migrationBuilder.CreateIndex(
                name: "IX_EducationYears_EducationTypeId_NameEn",
                table: "EducationYears",
                columns: new[] { "EducationTypeId", "NameEn" });

            migrationBuilder.CreateIndex(
                name: "IX_EducationYears_EducationTypeId_SortOrder",
                table: "EducationYears",
                columns: new[] { "EducationTypeId", "SortOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_EducationTypes_EducationTypeId",
                table: "Lessons",
                column: "EducationTypeId",
                principalTable: "EducationTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_EducationYears_EducationYearId",
                table: "Lessons",
                column: "EducationYearId",
                principalTable: "EducationYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_EducationTypes_EducationTypeId",
                table: "Lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_EducationYears_EducationYearId",
                table: "Lessons");

            migrationBuilder.DropTable(
                name: "EducationYears");

            migrationBuilder.DropTable(
                name: "EducationTypes");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_EducationTypeId",
                table: "Lessons");  

            migrationBuilder.DropIndex(
                name: "IX_Lessons_EducationYearId",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "EducationTypeId",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "EducationYearId",
                table: "Lessons");

            migrationBuilder.AddColumn<string>(
                name: "AcademicYear",
                table: "Lessons",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
