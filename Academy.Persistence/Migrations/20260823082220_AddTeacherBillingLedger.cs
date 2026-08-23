using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Academy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherBillingLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "LessonSessionStudentDetails");

            migrationBuilder.AddColumn<bool>(
                name: "ChargeAbsentSessions",
                table: "Lessons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMakeup",
                table: "LessonGroupSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MakeupForSessionId",
                table: "LessonGroupSessions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Charges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    LessonGroupId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LessonGroupSessionId = table.Column<int>(type: "int", nullable: true),
                    CycleStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CycleEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Settlement = table.Column<int>(type: "int", nullable: false),
                    ParentChargeId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Charges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Charges_Charges_ParentChargeId",
                        column: x => x.ParentChargeId,
                        principalTable: "Charges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Charges_LessonGroupSessions_LessonGroupSessionId",
                        column: x => x.LessonGroupSessionId,
                        principalTable: "LessonGroupSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Charges_LessonGroups_LessonGroupId",
                        column: x => x.LessonGroupId,
                        principalTable: "LessonGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Charges_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Charges_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Charges_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Method = table.Column<int>(type: "int", nullable: false),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiptNumber = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RecordedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentAllocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    ChargeId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentAllocations_Charges_ChargeId",
                        column: x => x.ChargeId,
                        principalTable: "Charges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentAllocations_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonGroupSessions_MakeupForSessionId",
                table: "LessonGroupSessions",
                column: "MakeupForSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Charges_LessonGroupId",
                table: "Charges",
                column: "LessonGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Charges_LessonGroupSessionId",
                table: "Charges",
                column: "LessonGroupSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Charges_LessonId_StudentId_Status",
                table: "Charges",
                columns: new[] { "LessonId", "StudentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Charges_LessonId_StudentId_Type_CycleStartDate",
                table: "Charges",
                columns: new[] { "LessonId", "StudentId", "Type", "CycleStartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Charges_ParentChargeId",
                table: "Charges",
                column: "ParentChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_Charges_StudentId",
                table: "Charges",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Charges_TeacherId_StudentId",
                table: "Charges",
                columns: new[] { "TeacherId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_ChargeId",
                table: "PaymentAllocations",
                column: "ChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_PaymentId_ChargeId",
                table: "PaymentAllocations",
                columns: new[] { "PaymentId", "ChargeId" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_LessonId_StudentId",
                table: "Payments",
                columns: new[] { "LessonId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaidAtUtc",
                table: "Payments",
                column: "PaidAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StudentId",
                table: "Payments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TeacherId_ReceiptNumber",
                table: "Payments",
                columns: new[] { "TeacherId", "ReceiptNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonGroupSessions_LessonGroupSessions_MakeupForSessionId",
                table: "LessonGroupSessions",
                column: "MakeupForSessionId",
                principalTable: "LessonGroupSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LessonGroupSessions_LessonGroupSessions_MakeupForSessionId",
                table: "LessonGroupSessions");

            migrationBuilder.DropTable(
                name: "PaymentAllocations");

            migrationBuilder.DropTable(
                name: "Charges");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_LessonGroupSessions_MakeupForSessionId",
                table: "LessonGroupSessions");

            migrationBuilder.DropColumn(
                name: "ChargeAbsentSessions",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "IsMakeup",
                table: "LessonGroupSessions");

            migrationBuilder.DropColumn(
                name: "MakeupForSessionId",
                table: "LessonGroupSessions");

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "LessonSessionStudentDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
