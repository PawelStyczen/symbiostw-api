using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceApi.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailNotificationLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailNotificationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PlainTextBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HtmlBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AzureOperationId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RequestedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuestUserId = table.Column<int>(type: "int", nullable: true),
                    MeetingId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailNotificationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailNotificationLogs_GuestUsers_GuestUserId",
                        column: x => x.GuestUserId,
                        principalTable: "GuestUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmailNotificationLogs_Meetings_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "Meetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationLogs_AzureOperationId",
                table: "EmailNotificationLogs",
                column: "AzureOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationLogs_GuestUserId",
                table: "EmailNotificationLogs",
                column: "GuestUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationLogs_MeetingId",
                table: "EmailNotificationLogs",
                column: "MeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationLogs_RecipientEmail",
                table: "EmailNotificationLogs",
                column: "RecipientEmail");

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationLogs_RequestedAtUtc",
                table: "EmailNotificationLogs",
                column: "RequestedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailNotificationLogs");
        }
    }
}
