using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TA_Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class identity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop the existing tables (Data is already gone, so this is safe)
            migrationBuilder.DropTable(name: "UserProjects");
            migrationBuilder.DropTable(name: "TaskMembers");

            // 2. Recreate UserProjects with Identity
            migrationBuilder.CreateTable(
    name: "UserProjects",
    columns: table => new
    {
        Id = table.Column<int>(type: "int", nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
        ProjectId = table.Column<int>(type: "int", nullable: false),
        Role = table.Column<string>(type: "nvarchar(max)", nullable: true),
        AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_UserProject", x => x.Id);
        table.ForeignKey(
            name: "FK_UserProject_AspNetUsers_UserId",
            column: x => x.UserId,
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
        table.ForeignKey(
            name: "FK_UserProject_Projects_ProjectId",
            column: x => x.ProjectId,
            principalTable: "Projects",
            principalColumn: "ProjectId",
            onDelete: ReferentialAction.Cascade);
    });

            migrationBuilder.CreateIndex(
                name: "IX_UserProject_ProjectId",
                table: "UserProject",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProject_UserId",
                table: "UserProject",
                column: "UserId");

            migrationBuilder.CreateTable(
     name: "TaskMembers",
     columns: table => new
     {
         Id = table.Column<int>(type: "int", nullable: false)
             .Annotation("SqlServer:Identity", "1, 1"),
         UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
         TaskId = table.Column<int>(type: "int", nullable: false),
         CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
         UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
         CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
         UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
         IsDeleted = table.Column<bool>(type: "bit", nullable: false)
     },
     constraints: table =>
     {
         table.PrimaryKey("PK_TaskMembers", x => x.Id);
         table.ForeignKey(
             name: "FK_TaskMembers_AspNetUsers_CreatedById",
             column: x => x.CreatedById,
             principalTable: "AspNetUsers",
             principalColumn: "Id",
             onDelete: ReferentialAction.Restrict);
         table.ForeignKey(
             name: "FK_TaskMembers_AspNetUsers_UpdatedById",
             column: x => x.UpdatedById,
             principalTable: "AspNetUsers",
             principalColumn: "Id",
             onDelete: ReferentialAction.Restrict);
         table.ForeignKey(
             name: "FK_TaskMembers_AspNetUsers_UserId",
             column: x => x.UserId,
             principalTable: "AspNetUsers",
             principalColumn: "Id",
             onDelete: ReferentialAction.Cascade);
         table.ForeignKey(
             name: "FK_TaskMembers_Tasks_TaskId",
             column: x => x.TaskId,
             principalTable: "Tasks",
             principalColumn: "TaskId",
             onDelete: ReferentialAction.Cascade);
     });

            migrationBuilder.CreateIndex(
                name: "IX_TaskMembers_CreatedById",
                table: "TaskMembers",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TaskMembers_TaskId",
                table: "TaskMembers",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskMembers_UpdatedById",
                table: "TaskMembers",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TaskMembers_UserId",
                table: "TaskMembers",
                column: "UserId");
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
