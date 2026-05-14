using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGreetings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCardDesignTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CardBackgrounds",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BgStyle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Categories = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsPremium = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardBackgrounds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CardDecorations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Preview = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Categories = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Elements = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardDecorations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardBackgrounds_IsActive",
                table: "CardBackgrounds",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CardDecorations_IsActive",
                table: "CardDecorations",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardBackgrounds");

            migrationBuilder.DropTable(
                name: "CardDecorations");
        }
    }
}
