using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceApi.Migrations
{
    /// <inheritdoc />
    public partial class instructorProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InstructorProfiles_AspNetUsers_CreatedById",
                table: "InstructorProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_InstructorProfiles_AspNetUsers_DeletedById",
                table: "InstructorProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_InstructorProfiles_AspNetUsers_UpdatedById",
                table: "InstructorProfiles");

            migrationBuilder.DropIndex(
                name: "IX_InstructorProfiles_CreatedById",
                table: "InstructorProfiles");

            migrationBuilder.DropIndex(
                name: "IX_InstructorProfiles_DeletedById",
                table: "InstructorProfiles");

            migrationBuilder.DropIndex(
                name: "IX_InstructorProfiles_UpdatedById",
                table: "InstructorProfiles");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "InstructorProfiles");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "InstructorProfiles");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "InstructorProfiles");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "InstructorProfiles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "InstructorProfiles");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "InstructorProfiles");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "InstructorProfiles");

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AllowNewsletter = table.Column<bool>(type: "bit", nullable: false),
                    AboutMe = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId",
                table: "UserProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "InstructorProfiles",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "InstructorProfiles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeletedById",
                table: "InstructorProfiles",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "InstructorProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "InstructorProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                table: "InstructorProfiles",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "InstructorProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstructorProfiles_CreatedById",
                table: "InstructorProfiles",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorProfiles_DeletedById",
                table: "InstructorProfiles",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorProfiles_UpdatedById",
                table: "InstructorProfiles",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_InstructorProfiles_AspNetUsers_CreatedById",
                table: "InstructorProfiles",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InstructorProfiles_AspNetUsers_DeletedById",
                table: "InstructorProfiles",
                column: "DeletedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InstructorProfiles_AspNetUsers_UpdatedById",
                table: "InstructorProfiles",
                column: "UpdatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
