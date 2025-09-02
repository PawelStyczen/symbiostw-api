using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceApi.Migrations
{
    /// <inheritdoc />
    public partial class isVisibleadd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsIndividual",
                table: "SubPages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsIndividual",
                table: "Reviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsIndividual",
                table: "NewsArticles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsIndividual",
                table: "MembershipPlans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsIndividual",
                table: "Meetings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsIndividual",
                table: "SubPages");

            migrationBuilder.DropColumn(
                name: "IsIndividual",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "IsIndividual",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "IsIndividual",
                table: "MembershipPlans");

            migrationBuilder.DropColumn(
                name: "IsIndividual",
                table: "Meetings");
        }
    }
}
