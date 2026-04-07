using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceApi.Migrations
{
    /// <inheritdoc />
    public partial class guestUserInstructorUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_AspNetUsers_InstructorId",
                table: "Meetings");

            migrationBuilder.AlterColumn<string>(
                name: "InstructorId",
                table: "Meetings",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "GuestInstructorId",
                table: "Meetings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GuestUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GuestInstructorProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuestUserId = table.Column<int>(type: "int", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ExperienceYears = table.Column<int>(type: "int", nullable: true),
                    Specialization = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FacebookLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InstagramLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TikTokLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShowOnPublicInstructorsPage = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestInstructorProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestInstructorProfiles_GuestUsers_GuestUserId",
                        column: x => x.GuestUserId,
                        principalTable: "GuestUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GuestUserProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuestUserId = table.Column<int>(type: "int", nullable: false),
                    AllowNewsletter = table.Column<bool>(type: "bit", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestUserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestUserProfiles_GuestUsers_GuestUserId",
                        column: x => x.GuestUserId,
                        principalTable: "GuestUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeetingGuestParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeetingId = table.Column<int>(type: "int", nullable: false),
                    GuestUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingGuestParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingGuestParticipants_GuestUsers_GuestUserId",
                        column: x => x.GuestUserId,
                        principalTable: "GuestUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MeetingGuestParticipants_Meetings_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "Meetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_GuestInstructorId",
                table: "Meetings",
                column: "GuestInstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestInstructorProfiles_GuestUserId",
                table: "GuestInstructorProfiles",
                column: "GuestUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuestUserProfiles_GuestUserId",
                table: "GuestUserProfiles",
                column: "GuestUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeetingGuestParticipants_GuestUserId",
                table: "MeetingGuestParticipants",
                column: "GuestUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingGuestParticipants_MeetingId",
                table: "MeetingGuestParticipants",
                column: "MeetingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_AspNetUsers_InstructorId",
                table: "Meetings",
                column: "InstructorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_GuestUsers_GuestInstructorId",
                table: "Meetings",
                column: "GuestInstructorId",
                principalTable: "GuestUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_AspNetUsers_InstructorId",
                table: "Meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_GuestUsers_GuestInstructorId",
                table: "Meetings");

            migrationBuilder.DropTable(
                name: "GuestInstructorProfiles");

            migrationBuilder.DropTable(
                name: "GuestUserProfiles");

            migrationBuilder.DropTable(
                name: "MeetingGuestParticipants");

            migrationBuilder.DropTable(
                name: "GuestUsers");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_GuestInstructorId",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "GuestInstructorId",
                table: "Meetings");

            migrationBuilder.AlterColumn<string>(
                name: "InstructorId",
                table: "Meetings",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_AspNetUsers_InstructorId",
                table: "Meetings",
                column: "InstructorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
