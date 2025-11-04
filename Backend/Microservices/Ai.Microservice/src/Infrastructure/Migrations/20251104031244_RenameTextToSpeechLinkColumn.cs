using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameTextToSpeechLinkColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "link",
                table: "text_to_speech_links");

            migrationBuilder.AddColumn<string>(
                name: "unique_id",
                table: "text_to_speech_links",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "idx_text_to_speech_links_unique_id",
                table: "text_to_speech_links",
                column: "unique_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_text_to_speech_links_unique_id",
                table: "text_to_speech_links");

            migrationBuilder.DropColumn(
                name: "unique_id",
                table: "text_to_speech_links");

            migrationBuilder.AddColumn<string>(
                name: "link",
                table: "text_to_speech_links",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");
        }
    }
}
