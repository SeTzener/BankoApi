using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankoApi.Migrations
{
    /// <inheritdoc />
    public partial class ExpenseTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExpenseTagId",
                table: "Transactions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExpenseTag",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<long>(type: "bigint", nullable: false),
                    Aka = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseTag", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ExpenseTagId",
                table: "Transactions",
                column: "ExpenseTagId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_ExpenseTag_ExpenseTagId",
                table: "Transactions",
                column: "ExpenseTagId",
                principalTable: "ExpenseTag",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_ExpenseTag_ExpenseTagId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "ExpenseTag");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ExpenseTagId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ExpenseTagId",
                table: "Transactions");
        }
    }
}
