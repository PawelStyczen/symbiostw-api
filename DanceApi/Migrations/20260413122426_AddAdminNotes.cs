using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TargetType = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DeletedById = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminNotes_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdminNotes_AspNetUsers_DeletedById",
                        column: x => x.DeletedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdminNotes_AspNetUsers_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotes_CreatedById",
                table: "AdminNotes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotes_CreatedDate",
                table: "AdminNotes",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotes_DeletedById",
                table: "AdminNotes",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotes_TargetType_TargetId_CreatedById",
                table: "AdminNotes",
                columns: new[] { "TargetType", "TargetId", "CreatedById" });

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotes_UpdatedById",
                table: "AdminNotes",
                column: "UpdatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminNotes");
        }
    }
}
