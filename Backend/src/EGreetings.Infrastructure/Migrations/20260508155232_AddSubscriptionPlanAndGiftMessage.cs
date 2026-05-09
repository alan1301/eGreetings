using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGreetings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionPlanAndGiftMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PendingGiftMessage",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Plan",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PendingGiftMessage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Plan",
                table: "Subscriptions");
        }
    }
}
