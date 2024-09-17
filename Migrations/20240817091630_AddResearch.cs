using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperInvestor.Migrations
{
    /// <inheritdoc />
    public partial class AddResearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ResearchId",
                table: "Notes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortId",
                table: "Notes",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Researches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShortId = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    Ticker = table.Column<string>(type: "text", nullable: true),
                    AccessionNumber = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Researches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Researches_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_ResearchId",
                table: "Notes",
                column: "ResearchId");

            migrationBuilder.CreateIndex(
                name: "IX_Researches_UserId",
                table: "Researches",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Researches_ResearchId",
                table: "Notes",
                column: "ResearchId",
                principalTable: "Researches",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Researches_ResearchId",
                table: "Notes");

            migrationBuilder.DropTable(
                name: "Researches");

            migrationBuilder.DropIndex(
                name: "IX_Notes_ResearchId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "ResearchId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "ShortId",
                table: "Notes");
        }
    }
}
