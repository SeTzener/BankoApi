using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddInstitutionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cleanup partial state from previous failed attempt & orphaned data
            migrationBuilder.Sql("""
                IF OBJECT_ID('FK_BankAuthorizations_Institutions_InstitutionId') IS NOT NULL
                    ALTER TABLE [BankAuthorizations] DROP CONSTRAINT [FK_BankAuthorizations_Institutions_InstitutionId];
                IF OBJECT_ID('FK_Transactions_BankAccounts_BankAccountId') IS NOT NULL
                    ALTER TABLE [Transactions] DROP CONSTRAINT [FK_Transactions_BankAccounts_BankAccountId];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BankAuthorizations_InstitutionId' AND object_id = OBJECT_ID('BankAuthorizations'))
                    DROP INDEX [IX_BankAuthorizations_InstitutionId] ON [BankAuthorizations];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Transactions_BankAccountId' AND object_id = OBJECT_ID('Transactions'))
                    DROP INDEX [IX_Transactions_BankAccountId] ON [Transactions];
                IF OBJECT_ID('Institutions') IS NOT NULL
                    DROP TABLE [Institutions];
                -- Remove orphaned transactions before creating FK
                DELETE FROM [Transactions]
                WHERE [BankAccountId] IS NOT NULL
                  AND NOT EXISTS (SELECT 1 FROM [BankAccounts] WHERE [Id] = [Transactions].[BankAccountId]);
            """);

            migrationBuilder.CreateTable(
                name: "Institutions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Institutions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BankAccountId",
                table: "Transactions",
                column: "BankAccountId");

            // Seed data from existing BankAuthorizations
            migrationBuilder.Sql(@"
                INSERT INTO [Institutions] ([Id], [Name], [CreatedAt], [UpdatedAt])
                SELECT DISTINCT
                    [ba].[InstitutionId],
                    COALESCE([ba].[InstitutionName], [ba].[InstitutionId]),
                    GETUTCDATE(),
                    GETUTCDATE()
                FROM [BankAuthorizations] [ba]
                WHERE [ba].[InstitutionId] IS NOT NULL
                  AND NOT EXISTS (SELECT 1 FROM [Institutions] [i] WHERE [i].[Id] = [ba].[InstitutionId])
            ");

            migrationBuilder.CreateIndex(
                name: "IX_BankAuthorizations_InstitutionId",
                table: "BankAuthorizations",
                column: "InstitutionId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankAuthorizations_Institutions_InstitutionId",
                table: "BankAuthorizations",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_BankAccounts_BankAccountId",
                table: "Transactions",
                column: "BankAccountId",
                principalTable: "BankAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankAuthorizations_Institutions_InstitutionId",
                table: "BankAuthorizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_BankAccounts_BankAccountId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "Institutions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_BankAccountId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_BankAuthorizations_InstitutionId",
                table: "BankAuthorizations");
        }
    }
}
