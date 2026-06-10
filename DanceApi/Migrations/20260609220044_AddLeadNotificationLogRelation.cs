using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceApi.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadNotificationLogRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LeadId",
                table: "NotificationLogs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_LeadId",
                table: "NotificationLogs",
                column: "LeadId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationLogs_Leads_LeadId",
                table: "NotificationLogs",
                column: "LeadId",
                principalTable: "Leads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationLogs_Leads_LeadId",
                table: "NotificationLogs");

            migrationBuilder.DropIndex(
                name: "IX_NotificationLogs_LeadId",
                table: "NotificationLogs");

            migrationBuilder.DropColumn(
                name: "LeadId",
                table: "NotificationLogs");
        }
    }
}
