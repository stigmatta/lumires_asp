using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNestedReviewComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentCommentId",
                table: "ReviewComments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewComments_ParentCommentId",
                table: "ReviewComments",
                column: "ParentCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewComments_ReviewComments_ParentCommentId",
                table: "ReviewComments",
                column: "ParentCommentId",
                principalTable: "ReviewComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReviewComments_ReviewComments_ParentCommentId",
                table: "ReviewComments");

            migrationBuilder.DropIndex(
                name: "IX_ReviewComments_ParentCommentId",
                table: "ReviewComments");

            migrationBuilder.DropColumn(
                name: "ParentCommentId",
                table: "ReviewComments");
        }
    }
}
