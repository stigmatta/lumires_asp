using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Updated_NotificationMessage_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 ALTER TABLE "UserNotifications" 
                                     ADD COLUMN IF NOT EXISTS "SenderAvatar" character varying(500),
                                     ADD COLUMN IF NOT EXISTS "SenderName" character varying(100),
                                     ADD COLUMN IF NOT EXISTS "TargetPayload" character varying(255);
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenderAvatar",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "SenderName",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "TargetPayload",
                table: "UserNotifications");
        }
    }
}
