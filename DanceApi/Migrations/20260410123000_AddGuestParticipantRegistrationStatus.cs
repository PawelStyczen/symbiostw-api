using DanceApi.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260410123000_AddGuestParticipantRegistrationStatus")]
    public partial class AddGuestParticipantRegistrationStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CreatedFromPublicRequest",
                table: "MeetingGuestParticipants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RegistrationStatus",
                table: "MeetingGuestParticipants",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedAt",
                table: "MeetingGuestParticipants",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedFromPublicRequest",
                table: "MeetingGuestParticipants");

            migrationBuilder.DropColumn(
                name: "RegistrationStatus",
                table: "MeetingGuestParticipants");

            migrationBuilder.DropColumn(
                name: "RequestedAt",
                table: "MeetingGuestParticipants");
        }
    }
}
