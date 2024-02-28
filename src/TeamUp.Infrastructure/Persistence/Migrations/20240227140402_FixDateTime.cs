using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamUp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixDateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "To",
                table: "Events",
                newName: "ToUtc");

            migrationBuilder.RenameColumn(
                name: "From",
                table: "Events",
                newName: "FromUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ToUtc",
                table: "Events",
                newName: "To");

            migrationBuilder.RenameColumn(
                name: "FromUtc",
                table: "Events",
                newName: "From");
        }
    }
}
