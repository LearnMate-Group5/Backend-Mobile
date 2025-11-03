using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUniqueConstraintToNameAndType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_subscriptions_type",
                table: "subscriptions");

            migrationBuilder.CreateIndex(
                name: "ux_subscriptions_name_type",
                table: "subscriptions",
                columns: new[] { "name", "type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_subscriptions_name_type",
                table: "subscriptions");

            migrationBuilder.CreateIndex(
                name: "ux_subscriptions_type",
                table: "subscriptions",
                column: "type",
                unique: true);
        }
    }
}
