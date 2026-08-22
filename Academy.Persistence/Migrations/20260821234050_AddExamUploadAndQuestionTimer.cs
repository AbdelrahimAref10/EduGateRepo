using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Academy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExamUploadAndQuestionTimer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SecondsPerQuestion",
                table: "Exams",
                type: "int",
                nullable: false,
                defaultValue: 600);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmittedAtUtc",
                table: "ExamAttempts",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "CurrentQuestionIndex",
                table: "ExamAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CurrentQuestionStartedAtUtc",
                table: "ExamAttempts",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecondsPerQuestion",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "CurrentQuestionIndex",
                table: "ExamAttempts");

            migrationBuilder.DropColumn(
                name: "CurrentQuestionStartedAtUtc",
                table: "ExamAttempts");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmittedAtUtc",
                table: "ExamAttempts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
