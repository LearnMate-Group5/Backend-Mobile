using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentUrlFieldsAndExpiredStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Deeplink",
                table: "payment_transaction",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "payment_transaction",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayUrl",
                table: "payment_transaction",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QrCodeUrl",
                table: "payment_transaction",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deeplink",
                table: "payment_transaction");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "payment_transaction");

            migrationBuilder.DropColumn(
                name: "PayUrl",
                table: "payment_transaction");

            migrationBuilder.DropColumn(
                name: "QrCodeUrl",
                table: "payment_transaction");
        }
    }
}
