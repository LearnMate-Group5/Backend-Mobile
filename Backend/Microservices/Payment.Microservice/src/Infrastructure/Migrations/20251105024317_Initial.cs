using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payment_transaction",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    order_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    request_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    amount = table.Column<long>(type: "bigint", nullable: false),
                    payment_gateway = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    order_info = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    result_code = table.Column<int>(type: "integer", nullable: false),
                    message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    app_trans_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    zp_trans_token = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    qr_code = table.Column<string>(type: "text", nullable: true),
                    momo_trans_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    pay_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    callback_data = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("payment_transaction_pkey", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_transaction");
        }
    }
}
