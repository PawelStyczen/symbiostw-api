using DanceApi.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260410120000_AddParticipantPaymentStatus")]
    public partial class AddParticipantPaymentStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasPaid",
                table: "MeetingParticipants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasPaid",
                table: "MeetingGuestParticipants",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasPaid",
                table: "MeetingParticipants");

            migrationBuilder.DropColumn(
                name: "HasPaid",
                table: "MeetingGuestParticipants");
        }
    }
}
