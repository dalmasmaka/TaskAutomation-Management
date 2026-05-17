using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TA_Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedUserGitHub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "UserGitHubConfigs");

            migrationBuilder.DropColumn(
                name: "ConnectedAt",
                table: "UserGitHubConfigs");

            migrationBuilder.DropColumn(
                name: "GitHubUserId",
                table: "UserGitHubConfigs");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UserGitHubConfigs");

            migrationBuilder.DropColumn(
                name: "TokenExpiresAt",
                table: "UserGitHubConfigs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "UserGitHubConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ConnectedAt",
                table: "UserGitHubConfigs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "GitHubUserId",
                table: "UserGitHubConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UserGitHubConfigs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenExpiresAt",
                table: "UserGitHubConfigs",
                type: "datetime2",
                nullable: true);
        }
    }
}
