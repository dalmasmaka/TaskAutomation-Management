using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TA_Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTeamTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTeam_AspNetUsers_CreatedById",
                table: "ProjectTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTeam_AspNetUsers_UpdatedById",
                table: "ProjectTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTeam_Projects_ProjectId",
                table: "ProjectTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTeam_Teams_TeamId",
                table: "ProjectTeam");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectTeam",
                table: "ProjectTeam");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Teams");

            migrationBuilder.RenameTable(
                name: "ProjectTeam",
                newName: "ProjectTeams");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectTeam_UpdatedById",
                table: "ProjectTeams",
                newName: "IX_ProjectTeams_UpdatedById");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectTeam_TeamId",
                table: "ProjectTeams",
                newName: "IX_ProjectTeams_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectTeam_ProjectId",
                table: "ProjectTeams",
                newName: "IX_ProjectTeams_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectTeam_CreatedById",
                table: "ProjectTeams",
                newName: "IX_ProjectTeams_CreatedById");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectTeams",
                table: "ProjectTeams",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTeams_AspNetUsers_CreatedById",
                table: "ProjectTeams",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTeams_AspNetUsers_UpdatedById",
                table: "ProjectTeams",
                column: "UpdatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTeams_Projects_ProjectId",
                table: "ProjectTeams",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTeams_Teams_TeamId",
                table: "ProjectTeams",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTeams_AspNetUsers_CreatedById",
                table: "ProjectTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTeams_AspNetUsers_UpdatedById",
                table: "ProjectTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTeams_Projects_ProjectId",
                table: "ProjectTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTeams_Teams_TeamId",
                table: "ProjectTeams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectTeams",
                table: "ProjectTeams");

            migrationBuilder.RenameTable(
                name: "ProjectTeams",
                newName: "ProjectTeam");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectTeams_UpdatedById",
                table: "ProjectTeam",
                newName: "IX_ProjectTeam_UpdatedById");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectTeams_TeamId",
                table: "ProjectTeam",
                newName: "IX_ProjectTeam_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectTeams_ProjectId",
                table: "ProjectTeam",
                newName: "IX_ProjectTeam_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectTeams_CreatedById",
                table: "ProjectTeam",
                newName: "IX_ProjectTeam_CreatedById");

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "Teams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectTeam",
                table: "ProjectTeam",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTeam_AspNetUsers_CreatedById",
                table: "ProjectTeam",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTeam_AspNetUsers_UpdatedById",
                table: "ProjectTeam",
                column: "UpdatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTeam_Projects_ProjectId",
                table: "ProjectTeam",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTeam_Teams_TeamId",
                table: "ProjectTeam",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
