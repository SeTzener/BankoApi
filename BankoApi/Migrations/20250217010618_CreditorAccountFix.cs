using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankoApi.Migrations
{
    /// <inheritdoc />
    public partial class CreditorAccountFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_CreditorAccount_CreditorAccountId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_ExpenseTag_ExpenseTagId",
                table: "Transactions");

            migrationBuilder.AlterColumn<string>(
                name: "Aka",
                table: "ExpenseTag",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Iban",
                table: "CreditorAccount",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseTag_Id",
                table: "ExpenseTag",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditorAccount_Iban",
                table: "CreditorAccount",
                column: "Iban");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_CreditorAccount_CreditorAccountId",
                table: "Transactions",
                column: "CreditorAccountId",
                principalTable: "CreditorAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_ExpenseTag_ExpenseTagId",
                table: "Transactions",
                column: "ExpenseTagId",
                principalTable: "ExpenseTag",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_CreditorAccount_CreditorAccountId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_ExpenseTag_ExpenseTagId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseTag_Id",
                table: "ExpenseTag");

            migrationBuilder.DropIndex(
                name: "IX_CreditorAccount_Iban",
                table: "CreditorAccount");

            migrationBuilder.AlterColumn<string>(
                name: "Aka",
                table: "ExpenseTag",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Iban",
                table: "CreditorAccount",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_CreditorAccount_CreditorAccountId",
                table: "Transactions",
                column: "CreditorAccountId",
                principalTable: "CreditorAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_ExpenseTag_ExpenseTagId",
                table: "Transactions",
                column: "ExpenseTagId",
                principalTable: "ExpenseTag",
                principalColumn: "Id");
        }
    }
}
