using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TA_Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProjectIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JiraIntegrations");

            migrationBuilder.CreateTable(
                name: "ProjectIntegrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectIntegrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectIntegrations_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectIntegrations_AspNetUsers_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectIntegrations_CreatedById",
                table: "ProjectIntegrations",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectIntegrations_UpdatedById",
                table: "ProjectIntegrations",
                column: "UpdatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectIntegrations");

            migrationBuilder.CreateTable(
                name: "JiraIntegrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CloudId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JiraIntegrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JiraIntegrations_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JiraIntegrations_AspNetUsers_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_JiraIntegrations_CreatedById",
                table: "JiraIntegrations",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_JiraIntegrations_UpdatedById",
                table: "JiraIntegrations",
                column: "UpdatedById");
        }
    }
}
