using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceApi.Migrations
{
    /// <inheritdoc />
    public partial class GeneralizeEmailNotificationLogsToNotificationLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailNotificationLogs_GuestUsers_GuestUserId",
                table: "EmailNotificationLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailNotificationLogs_Meetings_MeetingId",
                table: "EmailNotificationLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmailNotificationLogs",
                table: "EmailNotificationLogs");

            migrationBuilder.RenameTable(
                name: "EmailNotificationLogs",
                newName: "NotificationLogs");

            migrationBuilder.RenameColumn(
                name: "RecipientEmail",
                table: "NotificationLogs",
                newName: "Recipient");

            migrationBuilder.RenameColumn(
                name: "PlainTextBody",
                table: "NotificationLogs",
                newName: "PlainTextContent");

            migrationBuilder.RenameColumn(
                name: "HtmlBody",
                table: "NotificationLogs",
                newName: "HtmlContent");

            migrationBuilder.RenameColumn(
                name: "EmailType",
                table: "NotificationLogs",
                newName: "Kind");

            migrationBuilder.RenameColumn(
                name: "AzureOperationId",
                table: "NotificationLogs",
                newName: "ProviderOperationId");

            migrationBuilder.RenameIndex(
                name: "IX_EmailNotificationLogs_AzureOperationId",
                table: "NotificationLogs",
                newName: "IX_NotificationLogs_ProviderOperationId");

            migrationBuilder.RenameIndex(
                name: "IX_EmailNotificationLogs_GuestUserId",
                table: "NotificationLogs",
                newName: "IX_NotificationLogs_GuestUserId");

            migrationBuilder.RenameIndex(
                name: "IX_EmailNotificationLogs_MeetingId",
                table: "NotificationLogs",
                newName: "IX_NotificationLogs_MeetingId");

            migrationBuilder.RenameIndex(
                name: "IX_EmailNotificationLogs_RecipientEmail",
                table: "NotificationLogs",
                newName: "IX_NotificationLogs_Recipient");

            migrationBuilder.RenameIndex(
                name: "IX_EmailNotificationLogs_RequestedAtUtc",
                table: "NotificationLogs",
                newName: "IX_NotificationLogs_RequestedAtUtc");

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "NotificationLogs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "HtmlContent",
                table: "NotificationLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "Channel",
                table: "NotificationLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "NotificationLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "AzureCommunicationServicesEmail");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NotificationLogs",
                table: "NotificationLogs",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_Channel",
                table: "NotificationLogs",
                column: "Channel");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_Kind",
                table: "NotificationLogs",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_Status",
                table: "NotificationLogs",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationLogs_GuestUsers_GuestUserId",
                table: "NotificationLogs",
                column: "GuestUserId",
                principalTable: "GuestUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationLogs_Meetings_MeetingId",
                table: "NotificationLogs",
                column: "MeetingId",
                principalTable: "Meetings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationLogs_GuestUsers_GuestUserId",
                table: "NotificationLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationLogs_Meetings_MeetingId",
                table: "NotificationLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NotificationLogs",
                table: "NotificationLogs");

            migrationBuilder.DropIndex(
                name: "IX_NotificationLogs_Channel",
                table: "NotificationLogs");

            migrationBuilder.DropIndex(
                name: "IX_NotificationLogs_Kind",
                table: "NotificationLogs");

            migrationBuilder.DropIndex(
                name: "IX_NotificationLogs_Status",
                table: "NotificationLogs");

            migrationBuilder.DropColumn(
                name: "Channel",
                table: "NotificationLogs");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "NotificationLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "NotificationLogs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HtmlContent",
                table: "NotificationLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "Recipient",
                table: "NotificationLogs",
                newName: "RecipientEmail");

            migrationBuilder.RenameColumn(
                name: "ProviderOperationId",
                table: "NotificationLogs",
                newName: "AzureOperationId");

            migrationBuilder.RenameColumn(
                name: "PlainTextContent",
                table: "NotificationLogs",
                newName: "PlainTextBody");

            migrationBuilder.RenameColumn(
                name: "Kind",
                table: "NotificationLogs",
                newName: "EmailType");

            migrationBuilder.RenameColumn(
                name: "HtmlContent",
                table: "NotificationLogs",
                newName: "HtmlBody");

            migrationBuilder.RenameIndex(
                name: "IX_NotificationLogs_RequestedAtUtc",
                table: "NotificationLogs",
                newName: "IX_EmailNotificationLogs_RequestedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_NotificationLogs_Recipient",
                table: "NotificationLogs",
                newName: "IX_EmailNotificationLogs_RecipientEmail");

            migrationBuilder.RenameIndex(
                name: "IX_NotificationLogs_ProviderOperationId",
                table: "NotificationLogs",
                newName: "IX_EmailNotificationLogs_AzureOperationId");

            migrationBuilder.RenameIndex(
                name: "IX_NotificationLogs_MeetingId",
                table: "NotificationLogs",
                newName: "IX_EmailNotificationLogs_MeetingId");

            migrationBuilder.RenameIndex(
                name: "IX_NotificationLogs_GuestUserId",
                table: "NotificationLogs",
                newName: "IX_EmailNotificationLogs_GuestUserId");

            migrationBuilder.RenameTable(
                name: "NotificationLogs",
                newName: "EmailNotificationLogs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmailNotificationLogs",
                table: "EmailNotificationLogs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailNotificationLogs_GuestUsers_GuestUserId",
                table: "EmailNotificationLogs",
                column: "GuestUserId",
                principalTable: "GuestUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailNotificationLogs_Meetings_MeetingId",
                table: "EmailNotificationLogs",
                column: "MeetingId",
                principalTable: "Meetings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
