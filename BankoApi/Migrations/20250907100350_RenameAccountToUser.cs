using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankoApi.Migrations
{
    /// <inheritdoc />
    public partial class RenameAccountToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, check if there are any invalid foreign key references
            migrationBuilder.Sql(@"
                -- Check for Transactions with AccountId that doesn't exist in Accounts
                IF EXISTS (SELECT 1 FROM Transactions t 
                          LEFT JOIN Accounts a ON t.AccountId = a.AccountId 
                          WHERE a.AccountId IS NULL AND t.AccountId IS NOT NULL)
                BEGIN
                    -- Create temporary users for missing accounts
                    INSERT INTO Accounts (AccountId, Email, CreatedAt, UpdatedAt, IsActive, ConsentGiven)
                    SELECT DISTINCT t.AccountId, 
                           'migrated_' + CAST(t.AccountId AS NVARCHAR(36)) + '@example.com', 
                           GETDATE(), GETDATE(), 1, 0
                    FROM Transactions t
                    LEFT JOIN Accounts a ON t.AccountId = a.AccountId
                    WHERE a.AccountId IS NULL AND t.AccountId IS NOT NULL
                END
            ");

            // Temporarily drop the foreign key constraint to avoid conflicts during rename
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Accounts_AccountId",
                table: "Transactions");

            // Rename the table from Accounts to Users
            migrationBuilder.RenameTable(
                name: "Accounts",
                newName: "Users");

            // Rename the primary key column from AccountId to UserId
            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "Users",
                newName: "UserId");

            // Rename the foreign key column in Transactions
            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "Transactions",
                newName: "UserId");

            // Rename the index
            migrationBuilder.RenameIndex(
                name: "IX_Transactions_AccountId",
                table: "Transactions",
                newName: "IX_Transactions_UserId");

            // Rename the email index
            migrationBuilder.RenameIndex(
                name: "IX_Accounts_Email",
                table: "Users",
                newName: "IX_Users_Email");

            // Re-add the foreign key constraint with the new column names
            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the foreign key first
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions");

            // Reverse the renames
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Transactions",
                newName: "AccountId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions",
                newName: "IX_Transactions_AccountId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Users",
                newName: "AccountId");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Accounts");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "Accounts",
                newName: "IX_Accounts_Email");

            // Re-add the original foreign key
            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Accounts_AccountId",
                table: "Transactions",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}