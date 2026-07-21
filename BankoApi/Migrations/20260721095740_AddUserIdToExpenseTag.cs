using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToExpenseTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pendings");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "ExpenseTag",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ExpenseTag");

            migrationBuilder.CreateTable(
                name: "Pendings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BookingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RemittanceInformationUnstructured = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RemittanceInformationUnstructuredArray = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pendings", x => x.Id);
                });
        }
    }
}
