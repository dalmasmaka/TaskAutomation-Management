using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TA_Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class JiraIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JiraIntegrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CloudId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JiraIntegrations");
        }
    }
}
