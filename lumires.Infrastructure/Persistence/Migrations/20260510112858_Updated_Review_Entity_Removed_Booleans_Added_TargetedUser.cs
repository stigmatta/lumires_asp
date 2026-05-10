using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Updated_Review_Entity_Removed_Booleans_Added_TargetedUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFirstWatch",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "IsLongForm",
                table: "Reviews");

            migrationBuilder.AddColumn<Guid>(
                name: "TargetedUserId",
                table: "ReviewComments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewComments_TargetedUserId",
                table: "ReviewComments",
                column: "TargetedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewComments_Users_TargetedUserId",
                table: "ReviewComments",
                column: "TargetedUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReviewComments_Users_TargetedUserId",
                table: "ReviewComments");

            migrationBuilder.DropIndex(
                name: "IX_ReviewComments_TargetedUserId",
                table: "ReviewComments");

            migrationBuilder.DropColumn(
                name: "TargetedUserId",
                table: "ReviewComments");

            migrationBuilder.AddColumn<bool>(
                name: "IsFirstWatch",
                table: "Reviews",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLongForm",
                table: "Reviews",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
