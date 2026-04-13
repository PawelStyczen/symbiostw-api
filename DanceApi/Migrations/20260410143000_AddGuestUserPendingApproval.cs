using DanceApi.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260410143000_AddGuestUserPendingApproval")]
    public partial class AddGuestUserPendingApproval : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPendingApproval",
                table: "GuestUserProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPendingApproval",
                table: "GuestUserProfiles");
        }
    }
}
