using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueUserInstitutionIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "idx_bank_auth_user_institution",
                table: "BankAuthorizations",
                columns: new[] { "UserId", "InstitutionId" },
                unique: true,
                filter: "[InstitutionId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_bank_auth_user_institution",
                table: "BankAuthorizations");
        }
    }
}
