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
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_Users_SavedFilms_SavedFilmId' AND table_name = 'Users') THEN
                        ALTER TABLE ""Users"" DROP CONSTRAINT ""FK_Users_SavedFilms_SavedFilmId""; END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_Users_SavedLists_SavedListId' AND table_name = 'Users') THEN
                        ALTER TABLE ""Users"" DROP CONSTRAINT ""FK_Users_SavedLists_SavedListId""; END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_Users_WatchedFilms_WatchedFilmId' AND table_name = 'Users') THEN
                        ALTER TABLE ""Users"" DROP CONSTRAINT ""FK_Users_WatchedFilms_WatchedFilmId""; END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ""IX_Users_SavedFilmId"";
                DROP INDEX IF EXISTS ""IX_Users_SavedListId"";
                DROP INDEX IF EXISTS ""IX_Users_WatchedFilmId"";
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Users""
                    DROP COLUMN IF EXISTS ""SavedFilmId"",
                    DROP COLUMN IF EXISTS ""SavedListId"",
                    DROP COLUMN IF EXISTS ""WatchedFilmId"";
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Users""
                    ADD COLUMN IF NOT EXISTS ""Biography"" character varying(4000),
                    ADD COLUMN IF NOT EXISTS ""DisplayName"" character varying(100),
                    ADD COLUMN IF NOT EXISTS ""Location"" character varying(100),
                    ADD COLUMN IF NOT EXISTS ""Pronouns"" character varying(15) NOT NULL DEFAULT 'NotDefined',
                    ADD COLUMN IF NOT EXISTS ""Tagline"" character varying(255);
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""UsersSettings"" (
                    ""Id"" uuid NOT NULL,
                    ""UserId"" uuid NOT NULL,
                    ""ProfileVisibility"" text NOT NULL,
                    ""IsAnyoneCanFollow"" boolean NOT NULL,
                    ""IsWatchlistPublic"" boolean NOT NULL,
                    ""AreLikesPublic"" boolean NOT NULL,
                    ""AreRatingsShowInFeeds"" boolean NOT NULL,
                    ""NotifyNewFollower"" boolean NOT NULL,
                    ""NotifyLikesOnReviews"" boolean NOT NULL,
                    ""NotifyRepliesAndMentions"" boolean NOT NULL,
                    ""NotifyActivityFromFollowed"" boolean NOT NULL,
                    ""NotifySavesOnLists"" boolean NOT NULL,
                    ""NotifyWeeklyDigest"" boolean NOT NULL,
                    CONSTRAINT ""PK_UsersSettings"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_UsersSettings_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""Id"") ON DELETE CASCADE
                );
            ");

            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_UsersSettings_UserId"" ON ""UsersSettings"" (""UserId"");
            ");
            
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS ""UserFavoriteFilms"";
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""UserFavoriteFilms"" (
                    ""Id"" uuid NOT NULL,
                    ""UserSettingsId"" uuid NOT NULL,
                    ""FilmId"" uuid NOT NULL,
                    ""Order"" integer NOT NULL,
                    CONSTRAINT ""PK_UserFavoriteFilms"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_UserFavoriteFilms_Films_FilmId"" FOREIGN KEY (""FilmId"") REFERENCES ""Films"" (""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_UserFavoriteFilms_UsersSettings_UserSettingsId"" FOREIGN KEY (""UserSettingsId"") REFERENCES ""UsersSettings"" (""Id"") ON DELETE CASCADE
                );
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_UserFavoriteFilms_FilmId"" ON ""UserFavoriteFilms"" (""FilmId"");
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_UserFavoriteFilms_UserSettingsId"" ON ""UserFavoriteFilms"" (""UserSettingsId"");
            ");
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
