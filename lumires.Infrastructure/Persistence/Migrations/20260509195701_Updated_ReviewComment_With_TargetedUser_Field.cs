using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Updated_ReviewComment_With_TargetedUser_Field : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
