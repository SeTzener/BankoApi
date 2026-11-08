using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddBankAccountIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "BankAccounts",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "BankAccounts");
        }
    }
}
