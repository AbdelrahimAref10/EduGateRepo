using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Academy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceEducationTypeWithAcademicYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EducationStages_EducationTypes_EducationTypeId",
                table: "EducationStages");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_EducationTypes_EducationTypeId",
                table: "Lessons");

            migrationBuilder.DropTable(
                name: "EducationTypes");

            migrationBuilder.DropIndex(
                name: "IX_EducationYears_EducationStageId_NameEn",
                table: "EducationYears");

            migrationBuilder.DropIndex(
                name: "IX_EducationStages_EducationTypeId_NameEn",
                table: "EducationStages");

            migrationBuilder.DropIndex(
                name: "IX_EducationStages_EducationTypeId_SortOrder",
                table: "EducationStages");

            migrationBuilder.DropColumn(
                name: "EducationTypeId",
                table: "EducationStages");

            migrationBuilder.RenameColumn(
                name: "EducationTypeId",
                table: "Lessons",
                newName: "AcademicYearId");

            migrationBuilder.RenameIndex(
                name: "IX_Lessons_TeacherId_EducationTypeId",
                table: "Lessons",
                newName: "IX_Lessons_TeacherId_AcademicYearId");

            migrationBuilder.RenameIndex(
                name: "IX_Lessons_EducationTypeId",
                table: "Lessons",
                newName: "IX_Lessons_AcademicYearId");

            migrationBuilder.CreateTable(
                name: "AcademicYears",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicYears", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_TeacherId_AcademicYearId_EducationStageId",
                table: "Lessons",
                columns: new[] { "TeacherId", "AcademicYearId", "EducationStageId" });

            migrationBuilder.CreateIndex(
                name: "IX_EducationYears_EducationStageId_NameEn",
                table: "EducationYears",
                columns: new[] { "EducationStageId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EducationStages_NameEn",
                table: "EducationStages",
                column: "NameEn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EducationStages_SortOrder",
                table: "EducationStages",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_Name",
                table: "AcademicYears",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_SortOrder",
                table: "AcademicYears",
                column: "SortOrder");

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_AcademicYears_AcademicYearId",
                table: "Lessons",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_AcademicYears_AcademicYearId",
                table: "Lessons");

            migrationBuilder.DropTable(
                name: "AcademicYears");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_TeacherId_AcademicYearId_EducationStageId",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_EducationYears_EducationStageId_NameEn",
                table: "EducationYears");

            migrationBuilder.DropIndex(
                name: "IX_EducationStages_NameEn",
                table: "EducationStages");

            migrationBuilder.DropIndex(
                name: "IX_EducationStages_SortOrder",
                table: "EducationStages");

            migrationBuilder.RenameColumn(
                name: "AcademicYearId",
                table: "Lessons",
                newName: "EducationTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Lessons_TeacherId_AcademicYearId",
                table: "Lessons",
                newName: "IX_Lessons_TeacherId_EducationTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Lessons_AcademicYearId",
                table: "Lessons",
                newName: "IX_Lessons_EducationTypeId");

            migrationBuilder.AddColumn<int>(
                name: "EducationTypeId",
                table: "EducationStages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EducationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EducationYears_EducationStageId_NameEn",
                table: "EducationYears",
                columns: new[] { "EducationStageId", "NameEn" });

            migrationBuilder.CreateIndex(
                name: "IX_EducationStages_EducationTypeId_NameEn",
                table: "EducationStages",
                columns: new[] { "EducationTypeId", "NameEn" });

            migrationBuilder.CreateIndex(
                name: "IX_EducationStages_EducationTypeId_SortOrder",
                table: "EducationStages",
                columns: new[] { "EducationTypeId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_EducationTypes_NameEn",
                table: "EducationTypes",
                column: "NameEn");

            migrationBuilder.AddForeignKey(
                name: "FK_EducationStages_EducationTypes_EducationTypeId",
                table: "EducationStages",
                column: "EducationTypeId",
                principalTable: "EducationTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_EducationTypes_EducationTypeId",
                table: "Lessons",
                column: "EducationTypeId",
                principalTable: "EducationTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
