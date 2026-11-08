using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAgreementExpiryTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccessValidForDays",
                table: "BankAuthorizations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AgreementAcceptedAt",
                table: "BankAuthorizations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AgreementCreatedAt",
                table: "BankAuthorizations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AgreementExpiresAt",
                table: "BankAuthorizations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStatusCheckAt",
                table: "BankAuthorizations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_bank_auth_agreement_expires_at",
                table: "BankAuthorizations",
                column: "AgreementExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_bank_auth_agreement_expires_at",
                table: "BankAuthorizations");

            migrationBuilder.DropColumn(
                name: "AccessValidForDays",
                table: "BankAuthorizations");

            migrationBuilder.DropColumn(
                name: "AgreementAcceptedAt",
                table: "BankAuthorizations");

            migrationBuilder.DropColumn(
                name: "AgreementCreatedAt",
                table: "BankAuthorizations");

            migrationBuilder.DropColumn(
                name: "AgreementExpiresAt",
                table: "BankAuthorizations");

            migrationBuilder.DropColumn(
                name: "LastStatusCheckAt",
                table: "BankAuthorizations");
        }
    }
}
