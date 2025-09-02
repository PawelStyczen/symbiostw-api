using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceApi.Migrations
{
    /// <inheritdoc />
    public partial class baseEntityUpdateUserMngmnt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "TypeOfMeetings",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "Meetings",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "Locations",
                newName: "UpdatedDate");

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "TypeOfMeetings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "TypeOfMeetings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeletedById",
                table: "TypeOfMeetings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "TypeOfMeetings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                table: "TypeOfMeetings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Meetings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Meetings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeletedById",
                table: "Meetings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Meetings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                table: "Meetings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Locations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Locations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeletedById",
                table: "Locations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Locations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                table: "Locations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NewsArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AllowComments = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DeletedById = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsArticles_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NewsArticles_AspNetUsers_DeletedById",
                        column: x => x.DeletedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NewsArticles_AspNetUsers_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NewsComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NewsArticleId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DeletedById = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsComments_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NewsComments_AspNetUsers_DeletedById",
                        column: x => x.DeletedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NewsComments_AspNetUsers_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NewsComments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NewsComments_NewsArticles_NewsArticleId",
                        column: x => x.NewsArticleId,
                        principalTable: "NewsArticles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TypeOfMeetings_CreatedById",
                table: "TypeOfMeetings",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TypeOfMeetings_DeletedById",
                table: "TypeOfMeetings",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_TypeOfMeetings_UpdatedById",
                table: "TypeOfMeetings",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_CreatedById",
                table: "Meetings",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_DeletedById",
                table: "Meetings",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_UpdatedById",
                table: "Meetings",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_CreatedById",
                table: "Locations",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_DeletedById",
                table: "Locations",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_UpdatedById",
                table: "Locations",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_CreatedById",
                table: "NewsArticles",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_DeletedById",
                table: "NewsArticles",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_UpdatedById",
                table: "NewsArticles",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_NewsComments_CreatedById",
                table: "NewsComments",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_NewsComments_DeletedById",
                table: "NewsComments",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_NewsComments_NewsArticleId",
                table: "NewsComments",
                column: "NewsArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsComments_UpdatedById",
                table: "NewsComments",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_NewsComments_UserId",
                table: "NewsComments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_AspNetUsers_CreatedById",
                table: "Locations",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_AspNetUsers_DeletedById",
                table: "Locations",
                column: "DeletedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_AspNetUsers_UpdatedById",
                table: "Locations",
                column: "UpdatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_AspNetUsers_CreatedById",
                table: "Meetings",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_AspNetUsers_DeletedById",
                table: "Meetings",
                column: "DeletedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_AspNetUsers_UpdatedById",
                table: "Meetings",
                column: "UpdatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TypeOfMeetings_AspNetUsers_CreatedById",
                table: "TypeOfMeetings",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TypeOfMeetings_AspNetUsers_DeletedById",
                table: "TypeOfMeetings",
                column: "DeletedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TypeOfMeetings_AspNetUsers_UpdatedById",
                table: "TypeOfMeetings",
                column: "UpdatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_AspNetUsers_CreatedById",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_Locations_AspNetUsers_DeletedById",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_Locations_AspNetUsers_UpdatedById",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_AspNetUsers_CreatedById",
                table: "Meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_AspNetUsers_DeletedById",
                table: "Meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_AspNetUsers_UpdatedById",
                table: "Meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_TypeOfMeetings_AspNetUsers_CreatedById",
                table: "TypeOfMeetings");

            migrationBuilder.DropForeignKey(
                name: "FK_TypeOfMeetings_AspNetUsers_DeletedById",
                table: "TypeOfMeetings");

            migrationBuilder.DropForeignKey(
                name: "FK_TypeOfMeetings_AspNetUsers_UpdatedById",
                table: "TypeOfMeetings");

            migrationBuilder.DropTable(
                name: "NewsComments");

            migrationBuilder.DropTable(
                name: "NewsArticles");

            migrationBuilder.DropIndex(
                name: "IX_TypeOfMeetings_CreatedById",
                table: "TypeOfMeetings");

            migrationBuilder.DropIndex(
                name: "IX_TypeOfMeetings_DeletedById",
                table: "TypeOfMeetings");

            migrationBuilder.DropIndex(
                name: "IX_TypeOfMeetings_UpdatedById",
                table: "TypeOfMeetings");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_CreatedById",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_DeletedById",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_UpdatedById",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Locations_CreatedById",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_DeletedById",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_UpdatedById",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "TypeOfMeetings");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "TypeOfMeetings");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "TypeOfMeetings");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "TypeOfMeetings");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "TypeOfMeetings");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Locations");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "TypeOfMeetings",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "Meetings",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "Locations",
                newName: "DeletedAt");
        }
    }
}
