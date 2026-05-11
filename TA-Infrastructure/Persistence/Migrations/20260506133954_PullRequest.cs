using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TA_Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PullRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PullRequests_CodeGenerations_CodeGenId",
                table: "PullRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_PullRequests_PRId",
                table: "Reviews");

            migrationBuilder.DropTable(
                name: "CodeGenerations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PullRequests",
                table: "PullRequests");

            migrationBuilder.DropIndex(
                name: "IX_PullRequests_CodeGenId",
                table: "PullRequests");

            migrationBuilder.DropColumn(
                name: "PRId",
                table: "PullRequests");

            migrationBuilder.RenameColumn(
                name: "CodeGenId",
                table: "PullRequests",
                newName: "TaskId");

            migrationBuilder.AlterColumn<long>(
                name: "PRId",
                table: "Reviews",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PullRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "BranchName",
                table: "PullRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "PullRequests",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "PRDescription",
                table: "PullRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PRName",
                table: "PullRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PRUrl",
                table: "PullRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepoOwner",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PullRequests",
                table: "PullRequests",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequests_TaskId",
                table: "PullRequests",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_PullRequests_Tasks_TaskId",
                table: "PullRequests",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_PullRequests_PRId",
                table: "Reviews",
                column: "PRId",
                principalTable: "PullRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PullRequests_Tasks_TaskId",
                table: "PullRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_PullRequests_PRId",
                table: "Reviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PullRequests",
                table: "PullRequests");

            migrationBuilder.DropIndex(
                name: "IX_PullRequests_TaskId",
                table: "PullRequests");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PullRequests");

            migrationBuilder.DropColumn(
                name: "PRDescription",
                table: "PullRequests");

            migrationBuilder.DropColumn(
                name: "PRName",
                table: "PullRequests");

            migrationBuilder.DropColumn(
                name: "PRUrl",
                table: "PullRequests");

            migrationBuilder.DropColumn(
                name: "RepoOwner",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "PullRequests",
                newName: "CodeGenId");

            migrationBuilder.AlterColumn<int>(
                name: "PRId",
                table: "Reviews",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PullRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BranchName",
                table: "PullRequests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PRId",
                table: "PullRequests",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PullRequests",
                table: "PullRequests",
                column: "PRId");

            migrationBuilder.CreateTable(
                name: "CodeGenerations",
                columns: table => new
                {
                    CodeGenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GeneratedCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeGenerations", x => x.CodeGenId);
                    table.ForeignKey(
                        name: "FK_CodeGenerations_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CodeGenerations_AspNetUsers_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CodeGenerations_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "TaskId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PullRequests_CodeGenId",
                table: "PullRequests",
                column: "CodeGenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CodeGenerations_CreatedById",
                table: "CodeGenerations",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_CodeGenerations_TaskId",
                table: "CodeGenerations",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeGenerations_UpdatedById",
                table: "CodeGenerations",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_PullRequests_CodeGenerations_CodeGenId",
                table: "PullRequests",
                column: "CodeGenId",
                principalTable: "CodeGenerations",
                principalColumn: "CodeGenId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_PullRequests_PRId",
                table: "Reviews",
                column: "PRId",
                principalTable: "PullRequests",
                principalColumn: "PRId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
