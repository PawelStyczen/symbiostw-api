using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSmsMarketingConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowSmsMarketing",
                table: "UserProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowSmsMarketing",
                table: "GuestUserProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowSmsMarketing",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "AllowSmsMarketing",
                table: "GuestUserProfiles");
        }
    }
}
