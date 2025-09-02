using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceApi.Migrations
{
    /// <inheritdoc />
    public partial class VisibleHighlightBaseEntityupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHighlighted",
                table: "TypeOfMeetings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "TypeOfMeetings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHighlighted",
                table: "SubPages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "SubPages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHighlighted",
                table: "Reviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "Reviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHighlighted",
                table: "NewsArticles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "NewsArticles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHighlighted",
                table: "Meetings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "Meetings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHighlighted",
                table: "TypeOfMeetings");

            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "TypeOfMeetings");

            migrationBuilder.DropColumn(
                name: "IsHighlighted",
                table: "SubPages");

            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "SubPages");

            migrationBuilder.DropColumn(
                name: "IsHighlighted",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "IsHighlighted",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "IsHighlighted",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "Meetings");
        }
    }
}
