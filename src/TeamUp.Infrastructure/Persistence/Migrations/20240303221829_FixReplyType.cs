using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamUp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixReplyType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Reply_Message",
                table: "EventResponse",
                newName: "Message");

            migrationBuilder.AddColumn<int>(
                name: "ReplyType",
                table: "EventResponse",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReplyType",
                table: "EventResponse");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "EventResponse",
                newName: "Reply_Message");
        }
    }
}
