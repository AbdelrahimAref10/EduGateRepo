using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Academy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfilePhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePhoto",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePhoto",
                table: "Users");
        }
    }
}
