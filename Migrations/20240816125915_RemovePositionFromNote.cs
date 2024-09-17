using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperInvestor.Migrations
{
    /// <inheritdoc />
    public partial class RemovePositionFromNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "Notes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "Notes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
