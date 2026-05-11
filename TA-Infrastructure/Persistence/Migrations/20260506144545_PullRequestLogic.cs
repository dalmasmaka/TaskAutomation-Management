using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TA_Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PullRequestLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Platform",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "RepoOwner",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "RepoURl",
                table: "Projects");

            migrationBuilder.CreateTable(
                name: "GitHubConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepoOwner = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepoName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultBranch = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GitHubConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GitHubConfigs_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GitHubConfigs_AspNetUsers_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GitHubConfigs_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GitHubConfigs_CreatedById",
                table: "GitHubConfigs",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_GitHubConfigs_ProjectId",
                table: "GitHubConfigs",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_GitHubConfigs_UpdatedById",
                table: "GitHubConfigs",
                column: "UpdatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GitHubConfigs");

            migrationBuilder.AddColumn<string>(
                name: "Platform",
                table: "Projects",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RepoOwner",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepoURl",
                table: "Projects",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
