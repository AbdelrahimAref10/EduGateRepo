using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Academy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCityAndAreasTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cities_Countries_CountryId",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Cities_CityId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "CityId",
                table: "Users",
                newName: "AreaId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_CityId",
                table: "Users",
                newName: "IX_Users_AreaId");

            migrationBuilder.RenameColumn(
                name: "CountryId",
                table: "Cities",
                newName: "GovernorateId");

            migrationBuilder.RenameIndex(
                name: "IX_Cities_CountryId_NameEn",
                table: "Cities",
                newName: "IX_Cities_GovernorateId_NameEn");

            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Areas_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Governorates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Governorates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Governorates_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Areas_CityId_NameEn",
                table: "Areas",
                columns: new[] { "CityId", "NameEn" });

            migrationBuilder.CreateIndex(
                name: "IX_Governorates_CountryId_NameEn",
                table: "Governorates",
                columns: new[] { "CountryId", "NameEn" });

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_Governorates_GovernorateId",
                table: "Cities",
                column: "GovernorateId",
                principalTable: "Governorates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Areas_AreaId",
                table: "Users",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cities_Governorates_GovernorateId",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Areas_AreaId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Areas");

            migrationBuilder.DropTable(
                name: "Governorates");

            migrationBuilder.RenameColumn(
                name: "AreaId",
                table: "Users",
                newName: "CityId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_AreaId",
                table: "Users",
                newName: "IX_Users_CityId");

            migrationBuilder.RenameColumn(
                name: "GovernorateId",
                table: "Cities",
                newName: "CountryId");

            migrationBuilder.RenameIndex(
                name: "IX_Cities_GovernorateId_NameEn",
                table: "Cities",
                newName: "IX_Cities_CountryId_NameEn");

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_Countries_CountryId",
                table: "Cities",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Cities_CityId",
                table: "Users",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
