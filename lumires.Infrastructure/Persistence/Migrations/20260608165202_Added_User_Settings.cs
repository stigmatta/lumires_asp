using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Added_User_Settings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_SavedFilms_SavedFilmId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_SavedLists_SavedListId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_WatchedFilms_WatchedFilmId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_SavedFilmId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_SavedListId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_WatchedFilmId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SavedFilmId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SavedListId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WatchedFilmId",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "Biography",
                table: "Users",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pronouns",
                table: "Users",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "NotDefined");

            migrationBuilder.AddColumn<string>(
                name: "Tagline",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UsersSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileVisibility = table.Column<string>(type: "text", nullable: false),
                    IsAnyoneCanFollow = table.Column<bool>(type: "boolean", nullable: false),
                    IsWatchlistPublic = table.Column<bool>(type: "boolean", nullable: false),
                    AreLikesPublic = table.Column<bool>(type: "boolean", nullable: false),
                    AreRatingsShowInFeeds = table.Column<bool>(type: "boolean", nullable: false),
                    IsLikesPublic = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyNewFollower = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyLikesOnReviews = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyRepliesAndMentions = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyActivityFromFollowed = table.Column<bool>(type: "boolean", nullable: false),
                    NotifySavesOnLists = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyWeeklyDigest = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsersSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFavoriteFilms",
                columns: table => new
                {
                    FavoriteFilmsId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserSettingsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavoriteFilms", x => new { x.FavoriteFilmsId, x.UserSettingsId });
                    table.ForeignKey(
                        name: "FK_UserFavoriteFilms_Films_FavoriteFilmsId",
                        column: x => x.FavoriteFilmsId,
                        principalTable: "Films",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFavoriteFilms_UsersSettings_UserSettingsId",
                        column: x => x.UserSettingsId,
                        principalTable: "UsersSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteFilms_UserSettingsId",
                table: "UserFavoriteFilms",
                column: "UserSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_UsersSettings_UserId",
                table: "UsersSettings",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFavoriteFilms");

            migrationBuilder.DropTable(
                name: "UsersSettings");

            migrationBuilder.DropColumn(
                name: "Biography",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Pronouns",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Tagline",
                table: "Users");

            migrationBuilder.AddColumn<Guid>(
                name: "SavedFilmId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SavedListId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WatchedFilmId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_SavedFilmId",
                table: "Users",
                column: "SavedFilmId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SavedListId",
                table: "Users",
                column: "SavedListId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_WatchedFilmId",
                table: "Users",
                column: "WatchedFilmId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_SavedFilms_SavedFilmId",
                table: "Users",
                column: "SavedFilmId",
                principalTable: "SavedFilms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_SavedLists_SavedListId",
                table: "Users",
                column: "SavedListId",
                principalTable: "SavedLists",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_WatchedFilms_WatchedFilmId",
                table: "Users",
                column: "WatchedFilmId",
                principalTable: "WatchedFilms",
                principalColumn: "Id");
        }
    }
}
