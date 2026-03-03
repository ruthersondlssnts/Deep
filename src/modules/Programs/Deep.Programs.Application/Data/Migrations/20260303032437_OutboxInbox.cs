using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deep.Programs.Application.Data.Migrations;

/// <inheritdoc />
public partial class OutboxInbox : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "inbox_message_consumers",
            schema: "programs",
            columns: table => new
            {
                inbox_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_inbox_message_consumers", x => new { x.inbox_message_id, x.name }));

        migrationBuilder.CreateTable(
            name: "inbox_messages",
            schema: "programs",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                content = table.Column<string>(type: "jsonb", maxLength: 3000, nullable: false),
                occurred_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                processed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                error = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true)
            },
            constraints: table => table.PrimaryKey("pk_inbox_messages", x => x.id));

        migrationBuilder.CreateTable(
            name: "outbox_message_consumers",
            schema: "programs",
            columns: table => new
            {
                outbox_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_outbox_message_consumers", x => new { x.outbox_message_id, x.name }));

        migrationBuilder.CreateTable(
            name: "outbox_messages",
            schema: "programs",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                content = table.Column<string>(type: "jsonb", maxLength: 3000, nullable: false),
                occurred_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                processed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                error = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true)
            },
            constraints: table => table.PrimaryKey("pk_outbox_messages", x => x.id));

        migrationBuilder.CreateIndex(
            name: "ix_inbox_messages_processed_at_utc_occurred_at_utc",
            schema: "programs",
            table: "inbox_messages",
            columns: new[] { "processed_at_utc", "occurred_at_utc" },
            filter: "processed_at_utc IS NULL");

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_processed_at_utc_occurred_at_utc",
            schema: "programs",
            table: "outbox_messages",
            columns: new[] { "processed_at_utc", "occurred_at_utc" },
            filter: "processed_at_utc IS NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "inbox_message_consumers",
            schema: "programs");

        migrationBuilder.DropTable(
            name: "inbox_messages",
            schema: "programs");

        migrationBuilder.DropTable(
            name: "outbox_message_consumers",
            schema: "programs");

        migrationBuilder.DropTable(
            name: "outbox_messages",
            schema: "programs");
    }
}
