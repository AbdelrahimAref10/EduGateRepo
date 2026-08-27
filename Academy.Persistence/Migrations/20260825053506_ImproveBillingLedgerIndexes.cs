using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Academy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImproveBillingLedgerIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Payments_TeacherId_PaidAtUtc",
                table: "Payments",
                columns: new[] { "TeacherId", "PaidAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Charges_TeacherId_CreatedAtUtc",
                table: "Charges",
                columns: new[] { "TeacherId", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_TeacherId_PaidAtUtc",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Charges_TeacherId_CreatedAtUtc",
                table: "Charges");
        }
    }
}
