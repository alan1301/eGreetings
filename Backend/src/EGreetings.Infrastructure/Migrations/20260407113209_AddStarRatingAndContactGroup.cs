using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EGreetings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStarRatingAndContactGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drafts_GreetingTemplates_TemplateId",
                table: "Drafts");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailLogs_Greetings_GreetingId",
                table: "EmailLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Users_UserId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_GreetingTemplates_Categories_CategoryId",
                table: "GreetingTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_WebContents_Users_UpdatedByUserId",
                table: "WebContents");

            migrationBuilder.DropForeignKey(
                name: "FK_WebContents_WebContents_PreviousVersionId",
                table: "WebContents");

            migrationBuilder.AlterColumn<string>(
                name: "CssStyle",
                table: "GreetingTemplates",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "EmailLogs",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Drafts_GreetingTemplates_TemplateId",
                table: "Drafts",
                column: "TemplateId",
                principalTable: "GreetingTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailLogs_Greetings_GreetingId",
                table: "EmailLogs",
                column: "GreetingId",
                principalTable: "Greetings",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Users_UserId",
                table: "Feedbacks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_GreetingTemplates_Categories_CategoryId",
                table: "GreetingTemplates",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WebContents_Users_UpdatedByUserId",
                table: "WebContents",
                column: "UpdatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WebContents_WebContents_PreviousVersionId",
                table: "WebContents",
                column: "PreviousVersionId",
                principalTable: "WebContents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drafts_GreetingTemplates_TemplateId",
                table: "Drafts");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailLogs_Greetings_GreetingId",
                table: "EmailLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Users_UserId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_GreetingTemplates_Categories_CategoryId",
                table: "GreetingTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_WebContents_Users_UpdatedByUserId",
                table: "WebContents");

            migrationBuilder.DropForeignKey(
                name: "FK_WebContents_WebContents_PreviousVersionId",
                table: "WebContents");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Name",
                table: "Categories");

            migrationBuilder.AlterColumn<string>(
                name: "CssStyle",
                table: "GreetingTemplates",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "EmailLogs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_Drafts_GreetingTemplates_TemplateId",
                table: "Drafts",
                column: "TemplateId",
                principalTable: "GreetingTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailLogs_Greetings_GreetingId",
                table: "EmailLogs",
                column: "GreetingId",
                principalTable: "Greetings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Users_UserId",
                table: "Feedbacks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GreetingTemplates_Categories_CategoryId",
                table: "GreetingTemplates",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WebContents_Users_UpdatedByUserId",
                table: "WebContents",
                column: "UpdatedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WebContents_WebContents_PreviousVersionId",
                table: "WebContents",
                column: "PreviousVersionId",
                principalTable: "WebContents",
                principalColumn: "Id");
        }
    }
}
