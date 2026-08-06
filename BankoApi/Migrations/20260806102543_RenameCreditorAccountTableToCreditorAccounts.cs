using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankoApi.Migrations
{
    /// <inheritdoc />
    public partial class RenameCreditorAccountTableToCreditorAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_CreditorAccount_CreditorAccountId",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CreditorAccount",
                table: "CreditorAccount");

            migrationBuilder.RenameTable(
                name: "CreditorAccount",
                newName: "CreditorAccounts");

            migrationBuilder.RenameIndex(
                name: "IX_CreditorAccount_Iban",
                table: "CreditorAccounts",
                newName: "IX_CreditorAccounts_Iban");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CreditorAccounts",
                table: "CreditorAccounts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_CreditorAccounts_CreditorAccountId",
                table: "Transactions",
                column: "CreditorAccountId",
                principalTable: "CreditorAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_CreditorAccounts_CreditorAccountId",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CreditorAccounts",
                table: "CreditorAccounts");

            migrationBuilder.RenameTable(
                name: "CreditorAccounts",
                newName: "CreditorAccount");

            migrationBuilder.RenameIndex(
                name: "IX_CreditorAccounts_Iban",
                table: "CreditorAccount",
                newName: "IX_CreditorAccount_Iban");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CreditorAccount",
                table: "CreditorAccount",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_CreditorAccount_CreditorAccountId",
                table: "Transactions",
                column: "CreditorAccountId",
                principalTable: "CreditorAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
