using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddGDPRTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ConsentVersionId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PrivacyPolicyVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivacyPolicyVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsentLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrivacyPolicyVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Accepted = table.Column<bool>(type: "bit", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsentLogs_PrivacyPolicyVersions_PrivacyPolicyVersionId",
                        column: x => x.PrivacyPolicyVersionId,
                        principalTable: "PrivacyPolicyVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsentLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ConsentVersionId",
                table: "Users",
                column: "ConsentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentLogs_PrivacyPolicyVersionId",
                table: "ConsentLogs",
                column: "PrivacyPolicyVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentLogs_UserId",
                table: "ConsentLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivacyPolicyVersions_Version",
                table: "PrivacyPolicyVersions",
                column: "Version",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_PrivacyPolicyVersions_ConsentVersionId",
                table: "Users",
                column: "ConsentVersionId",
                principalTable: "PrivacyPolicyVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_PrivacyPolicyVersions_ConsentVersionId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "ConsentLogs");

            migrationBuilder.DropTable(
                name: "PrivacyPolicyVersions");

            migrationBuilder.DropIndex(
                name: "IX_Users_ConsentVersionId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ConsentVersionId",
                table: "Users");
        }
    }
}
