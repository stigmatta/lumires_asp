using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Added_WatchedFilm_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonsLocalizations_People_PersonId1",
                table: "PersonsLocalizations");

            migrationBuilder.DropIndex(
                name: "IX_PersonsLocalizations_PersonId1",
                table: "PersonsLocalizations");

            migrationBuilder.DropColumn(
                name: "PersonId1",
                table: "PersonsLocalizations");

            migrationBuilder.AddColumn<Guid>(
                name: "WatchedFilmId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WatchedFilms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FilmId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchedFilms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WatchedFilms_Films_FilmId",
                        column: x => x.FilmId,
                        principalTable: "Films",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WatchedFilms_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_WatchedFilmId",
                table: "Users",
                column: "WatchedFilmId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchedFilms_FilmId",
                table: "WatchedFilms",
                column: "FilmId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchedFilms_UserId_FilmId",
                table: "WatchedFilms",
                columns: new[] { "UserId", "FilmId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_WatchedFilms_WatchedFilmId",
                table: "Users",
                column: "WatchedFilmId",
                principalTable: "WatchedFilms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_WatchedFilms_WatchedFilmId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "WatchedFilms");

            migrationBuilder.DropIndex(
                name: "IX_Users_WatchedFilmId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WatchedFilmId",
                table: "Users");

            migrationBuilder.AddColumn<Guid>(
                name: "PersonId1",
                table: "PersonsLocalizations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonsLocalizations_PersonId1",
                table: "PersonsLocalizations",
                column: "PersonId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonsLocalizations_People_PersonId1",
                table: "PersonsLocalizations",
                column: "PersonId1",
                principalTable: "People",
                principalColumn: "Id");
        }
    }
}
