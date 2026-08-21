using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Academy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEductionStagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EducationYears_EducationTypes_EducationTypeId",
                table: "EducationYears");

            migrationBuilder.RenameColumn(
                name: "EducationTypeId",
                table: "EducationYears",
                newName: "EducationStageId");

            migrationBuilder.RenameIndex(
                name: "IX_EducationYears_EducationTypeId_SortOrder",
                table: "EducationYears",
                newName: "IX_EducationYears_EducationStageId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_EducationYears_EducationTypeId_NameEn",
                table: "EducationYears",
                newName: "IX_EducationYears_EducationStageId_NameEn");

            migrationBuilder.AddColumn<int>(
                name: "EducationStageId",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EducationSubjectId",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EducationStages",
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
                    table.PrimaryKey("PK_EducationStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EducationStages_EducationTypes_EducationTypeId",
                        column: x => x.EducationTypeId,
                        principalTable: "EducationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EducationSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EducationYearId = table.Column<int>(type: "int", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EducationSubjects_EducationYears_EducationYearId",
                        column: x => x.EducationYearId,
                        principalTable: "EducationYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_EducationStageId",
                table: "Lessons",
                column: "EducationStageId");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_EducationSubjectId",
                table: "Lessons",
                column: "EducationSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_EducationStages_EducationTypeId_NameEn",
                table: "EducationStages",
                columns: new[] { "EducationTypeId", "NameEn" });

            migrationBuilder.CreateIndex(
                name: "IX_EducationStages_EducationTypeId_SortOrder",
                table: "EducationStages",
                columns: new[] { "EducationTypeId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_EducationSubjects_EducationYearId_NameEn",
                table: "EducationSubjects",
                columns: new[] { "EducationYearId", "NameEn" });

            migrationBuilder.CreateIndex(
                name: "IX_EducationSubjects_EducationYearId_SortOrder",
                table: "EducationSubjects",
                columns: new[] { "EducationYearId", "SortOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_EducationYears_EducationStages_EducationStageId",
                table: "EducationYears",
                column: "EducationStageId",
                principalTable: "EducationStages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_EducationStages_EducationStageId",
                table: "Lessons",
                column: "EducationStageId",
                principalTable: "EducationStages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_EducationSubjects_EducationSubjectId",
                table: "Lessons",
                column: "EducationSubjectId",
                principalTable: "EducationSubjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EducationYears_EducationStages_EducationStageId",
                table: "EducationYears");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_EducationStages_EducationStageId",
                table: "Lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_EducationSubjects_EducationSubjectId",
                table: "Lessons");

            migrationBuilder.DropTable(
                name: "EducationStages");

            migrationBuilder.DropTable(
                name: "EducationSubjects");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_EducationStageId",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_EducationSubjectId",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "EducationStageId",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "EducationSubjectId",
                table: "Lessons");

            migrationBuilder.RenameColumn(
                name: "EducationStageId",
                table: "EducationYears",
                newName: "EducationTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_EducationYears_EducationStageId_SortOrder",
                table: "EducationYears",
                newName: "IX_EducationYears_EducationTypeId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_EducationYears_EducationStageId_NameEn",
                table: "EducationYears",
                newName: "IX_EducationYears_EducationTypeId_NameEn");

            migrationBuilder.AddForeignKey(
                name: "FK_EducationYears_EducationTypes_EducationTypeId",
                table: "EducationYears",
                column: "EducationTypeId",
                principalTable: "EducationTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
