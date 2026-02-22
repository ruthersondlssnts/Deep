using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deep.Accounts.Application.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_refresh_tokens_accounts_account_id",
                schema: "accounts",
                table: "refresh_tokens");

            migrationBuilder.CreateTable(
                name: "password_histories",
                schema: "accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    changed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_password_histories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                schema: "accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expiry_date_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    used_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_password_reset_tokens", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_password_histories_account_id",
                schema: "accounts",
                table: "password_histories",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_password_histories_account_id_changed_at_utc",
                schema: "accounts",
                table: "password_histories",
                columns: new[] { "account_id", "changed_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_password_reset_tokens_account_id",
                schema: "accounts",
                table: "password_reset_tokens",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_password_reset_tokens_token",
                schema: "accounts",
                table: "password_reset_tokens",
                column: "token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "password_histories",
                schema: "accounts");

            migrationBuilder.DropTable(
                name: "password_reset_tokens",
                schema: "accounts");

            migrationBuilder.AddForeignKey(
                name: "fk_refresh_tokens_accounts_account_id",
                schema: "accounts",
                table: "refresh_tokens",
                column: "account_id",
                principalSchema: "accounts",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
