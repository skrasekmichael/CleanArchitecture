using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamUp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixConcurrencyAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Users",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "TeamMember",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Events",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_TeamId_RecipientId",
                table: "Invitations",
                columns: new[] { "TeamId", "RecipientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventResponse_EventId_TeamMemberId",
                table: "EventResponse",
                columns: new[] { "EventId", "TeamMemberId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invitations_TeamId_RecipientId",
                table: "Invitations");

            migrationBuilder.DropIndex(
                name: "IX_EventResponse_EventId_TeamMemberId",
                table: "EventResponse");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "TeamMember");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Events");
        }
    }
}
