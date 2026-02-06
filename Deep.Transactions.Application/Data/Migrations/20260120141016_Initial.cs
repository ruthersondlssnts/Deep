using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deep.Transactions.Application.Data.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "transactions");

        migrationBuilder.CreateTable(
            name: "customers",
            schema: "transactions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                full_name = table.Column<string>(
                    type: "character varying(200)",
                    maxLength: 200,
                    nullable: false
                ),
                email = table.Column<string>(
                    type: "character varying(320)",
                    maxLength: 320,
                    nullable: false
                ),
            },
            constraints: table => table.PrimaryKey("pk_customers", x => x.id));

        migrationBuilder.CreateTable(
            name: "transactions",
            schema: "transactions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                program_id = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_transactions", x => x.id);
                table.ForeignKey(
                    name: "fk_transactions_customers_customer_id",
                    column: x => x.customer_id,
                    principalSchema: "transactions",
                    principalTable: "customers",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict
                );
            }
        );

        migrationBuilder.CreateIndex(
            name: "ix_customers_email",
            schema: "transactions",
            table: "customers",
            column: "email",
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "ix_transactions_customer_id",
            schema: "transactions",
            table: "transactions",
            column: "customer_id"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "transactions", schema: "transactions");

        migrationBuilder.DropTable(name: "customers", schema: "transactions");
    }
}
