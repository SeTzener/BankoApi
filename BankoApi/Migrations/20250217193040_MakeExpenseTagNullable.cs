using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankoApi.Migrations
{
    /// <inheritdoc />
    public partial class MakeExpenseTagNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_ExpenseTag_ExpenseTagId",
                table: "Transactions");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_ExpenseTag_ExpenseTagId",
                table: "Transactions",
                column: "ExpenseTagId",
                principalTable: "ExpenseTag",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_ExpenseTag_ExpenseTagId",
                table: "Transactions");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_ExpenseTag_ExpenseTagId",
                table: "Transactions",
                column: "ExpenseTagId",
                principalTable: "ExpenseTag",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
