using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "InternalTransactionId",
                table: "Transactions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_InternalTransactionId",
                table: "Transactions",
                column: "InternalTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId_BookingDate",
                table: "Transactions",
                columns: new[] { "UserId", "BookingDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_InternalTransactionId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserId_BookingDate",
                table: "Transactions");

            migrationBuilder.AlterColumn<string>(
                name: "InternalTransactionId",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
