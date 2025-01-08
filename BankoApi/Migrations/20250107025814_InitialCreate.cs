using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankoApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Balances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BalanceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Balances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CreditorAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Iban = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Bban = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditorAccount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DebtorAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Iban = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Bban = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebtorAccount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Institutions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BIC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionTotalDays = table.Column<int>(type: "int", nullable: false),
                    Countries = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Logo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxAccessValidForDays = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Institutions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Requisitions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Redirect = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstitutionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Agreement = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Accounts = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserLanguage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Link = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SSN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountSelection = table.Column<bool>(type: "bit", nullable: false),
                    RedirectImmediate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requisitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionAmount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionAmount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankTransactionsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_BankTransactions_BankTransactionsId",
                        column: x => x.BankTransactionsId,
                        principalTable: "BankTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Booked",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BookingDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValueDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionAmountId = table.Column<int>(type: "int", nullable: false),
                    DebtorAccountId = table.Column<int>(type: "int", nullable: true),
                    RemittanceInformationUnstructured = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RemittanceInformationUnstructuredArray = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankTransactionCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InternalTransactionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreditorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreditorAccountId = table.Column<int>(type: "int", nullable: true),
                    DebtorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RemittanceInformationStructuredArray = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankTransactionsId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Booked", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Booked_BankTransactions_BankTransactionsId",
                        column: x => x.BankTransactionsId,
                        principalTable: "BankTransactions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Booked_CreditorAccount_CreditorAccountId",
                        column: x => x.CreditorAccountId,
                        principalTable: "CreditorAccount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Booked_DebtorAccount_DebtorAccountId",
                        column: x => x.DebtorAccountId,
                        principalTable: "DebtorAccount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Booked_TransactionAmount_TransactionAmountId",
                        column: x => x.TransactionAmountId,
                        principalTable: "TransactionAmount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pending",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionAmountId = table.Column<int>(type: "int", nullable: false),
                    RemittanceInformationUnstructured = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RemittanceInformationUnstructuredArray = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankTransactionsId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pending", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pending_BankTransactions_BankTransactionsId",
                        column: x => x.BankTransactionsId,
                        principalTable: "BankTransactions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Pending_TransactionAmount_TransactionAmountId",
                        column: x => x.TransactionAmountId,
                        principalTable: "TransactionAmount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Booked_BankTransactionsId",
                table: "Booked",
                column: "BankTransactionsId");

            migrationBuilder.CreateIndex(
                name: "IX_Booked_CreditorAccountId",
                table: "Booked",
                column: "CreditorAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Booked_DebtorAccountId",
                table: "Booked",
                column: "DebtorAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Booked_TransactionAmountId",
                table: "Booked",
                column: "TransactionAmountId");

            migrationBuilder.CreateIndex(
                name: "IX_Pending_BankTransactionsId",
                table: "Pending",
                column: "BankTransactionsId");

            migrationBuilder.CreateIndex(
                name: "IX_Pending_TransactionAmountId",
                table: "Pending",
                column: "TransactionAmountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BankTransactionsId",
                table: "Transactions",
                column: "BankTransactionsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Balances");

            migrationBuilder.DropTable(
                name: "Booked");

            migrationBuilder.DropTable(
                name: "Institutions");

            migrationBuilder.DropTable(
                name: "Pending");

            migrationBuilder.DropTable(
                name: "Requisitions");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "CreditorAccount");

            migrationBuilder.DropTable(
                name: "DebtorAccount");

            migrationBuilder.DropTable(
                name: "TransactionAmount");

            migrationBuilder.DropTable(
                name: "BankTransactions");
        }
    }
}
