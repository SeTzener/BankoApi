using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankoApi.Migrations
{
    /// <inheritdoc />
    public partial class GoCardlessBankAuthorization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_bank_auth_status",
                table: "bankAuthorizations");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "BankAccounts",
                newName: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_BankAuthorizationId_BankAccountId",
                table: "BankAccounts",
                columns: new[] { "BankAuthorizationId", "BankAccountId" },
                unique: true);

            // 2. Change Status column from string to int (for enum storage)
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "bankAuthorizations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            // 3. Add length constraints to existing string columns
            migrationBuilder.AlterColumn<string>(
                name: "RequisitionId",
                table: "bankAuthorizations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InstitutionId",
                table: "bankAuthorizations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AgreementId",
                table: "bankAuthorizations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InstitutionName",
                table: "bankAuthorizations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            // 4. Remove old columns that are no longer in the model
            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "bankAuthorizations");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "bankAuthorizations");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "bankAuthorizations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_BankAuthorizationId_BankAccountId",
                table: "BankAccounts");

            migrationBuilder.RenameColumn(
                name: "BankAccountId",
                table: "BankAccounts",
                newName: "AccountId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "bankAuthorizations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "RequisitionId",
                table: "bankAuthorizations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "InstitutionId",
                table: "bankAuthorizations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "AgreementId",
                table: "bankAuthorizations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InstitutionName",
                table: "bankAuthorizations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                table: "bankAuthorizations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "bankAuthorizations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "bankAuthorizations",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}