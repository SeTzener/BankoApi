using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankoApi.Migrations
{
    /// <inheritdoc />
    public partial class DatesFromStringToDateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DebtorAccount",
                table: "Transactions",
                newName: "DebtorAccountId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ValueDate",
                table: "Transactions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookingDate",
                table: "Transactions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookingDate",
                table: "Pendings",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Iban",
                table: "DebtorAccounts",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_DebtorAccountId",
                table: "Transactions",
                column: "DebtorAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_DebtorAccounts_Iban",
                table: "DebtorAccounts",
                column: "Iban",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_DebtorAccounts_DebtorAccountId",
                table: "Transactions",
                column: "DebtorAccountId",
                principalTable: "DebtorAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_DebtorAccounts_DebtorAccountId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_DebtorAccountId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_DebtorAccounts_Iban",
                table: "DebtorAccounts");

            migrationBuilder.RenameColumn(
                name: "DebtorAccountId",
                table: "Transactions",
                newName: "DebtorAccount");

            migrationBuilder.AlterColumn<string>(
                name: "ValueDate",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "BookingDate",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "BookingDate",
                table: "Pendings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Iban",
                table: "DebtorAccounts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
