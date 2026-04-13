using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TargetType = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    ActorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ActorDisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ActorIdentifier = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActionType",
                table: "AuditLogs",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ChangedAtUtc",
                table: "AuditLogs",
                column: "ChangedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_SourceType",
                table: "AuditLogs",
                column: "SourceType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TargetType_TargetId_ChangedAtUtc",
                table: "AuditLogs",
                columns: new[] { "TargetType", "TargetId", "ChangedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");
        }
    }
}
