using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddEuaNotificationTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryExpiredNotifiedFor",
                table: "BankAuthorizations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryWarningNotifiedFor",
                table: "BankAuthorizations",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiryExpiredNotifiedFor",
                table: "BankAuthorizations");

            migrationBuilder.DropColumn(
                name: "ExpiryWarningNotifiedFor",
                table: "BankAuthorizations");
        }
    }
}
