using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankoApi.Migrations
{
    /// <inheritdoc />
    public partial class GoCardlessIntegrationUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "bankAuthorizations");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "bankAuthorizations");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "bankAuthorizations");

            migrationBuilder.AddColumn<bool>(
                name: "IsAgreementExpired",
                table: "bankAuthorizations",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAgreementExpired",
                table: "bankAuthorizations");

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
