using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Academy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RedesignNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Notifications",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Notifications",
                newName: "TitleEn");

            migrationBuilder.RenameColumn(
                name: "Body",
                table: "Notifications",
                newName: "BodyEn");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                newName: "IX_Notifications_Type");

            migrationBuilder.AddColumn<string>(
                name: "BodyAr",
                table: "Notifications",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityType",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                table: "Notifications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UserTargetId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NotificationDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationDetails_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationDetails_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAtUtc",
                table: "Notifications",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_EntityType_EntityId",
                table: "Notifications",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserTargetId",
                table: "Notifications",
                column: "UserTargetId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDetails_NotificationId_UserId",
                table: "NotificationDetails",
                columns: new[] { "NotificationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDetails_UserId_IsRead_CreatedAtUtc",
                table: "NotificationDetails",
                columns: new[] { "UserId", "IsRead", "CreatedAtUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserTargetId",
                table: "Notifications",
                column: "UserTargetId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserTargetId",
                table: "Notifications");

            migrationBuilder.DropTable(
                name: "NotificationDetails");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CreatedAtUtc",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_EntityType_EntityId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserTargetId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "BodyAr",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "UserTargetId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Notifications",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "TitleEn",
                table: "Notifications",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "BodyEn",
                table: "Notifications",
                newName: "Body");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_Type",
                table: "Notifications",
                newName: "IX_Notifications_UserId");

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
