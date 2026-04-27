using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TA_Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedAuditToConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CompanyIntegrationConfigs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "CompanyIntegrationConfigs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CompanyIntegrationConfigs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CompanyIntegrationConfigs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                table: "CompanyIntegrationConfigs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyIntegrationConfigs_CreatedById",
                table: "CompanyIntegrationConfigs",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyIntegrationConfigs_UpdatedById",
                table: "CompanyIntegrationConfigs",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyIntegrationConfigs_AspNetUsers_CreatedById",
                table: "CompanyIntegrationConfigs",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyIntegrationConfigs_AspNetUsers_UpdatedById",
                table: "CompanyIntegrationConfigs",
                column: "UpdatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyIntegrationConfigs_AspNetUsers_CreatedById",
                table: "CompanyIntegrationConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanyIntegrationConfigs_AspNetUsers_UpdatedById",
                table: "CompanyIntegrationConfigs");

            migrationBuilder.DropIndex(
                name: "IX_CompanyIntegrationConfigs_CreatedById",
                table: "CompanyIntegrationConfigs");

            migrationBuilder.DropIndex(
                name: "IX_CompanyIntegrationConfigs_UpdatedById",
                table: "CompanyIntegrationConfigs");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CompanyIntegrationConfigs");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "CompanyIntegrationConfigs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CompanyIntegrationConfigs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CompanyIntegrationConfigs");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "CompanyIntegrationConfigs");
        }
    }
}
