using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deep.Programs.Application.Data.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "programs");

        migrationBuilder.CreateTable(
            name: "programs",
            schema: "programs",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                program_status = table.Column<string>(type: "text", nullable: false),
                starts_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ends_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                owner_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_programs", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "users",
            schema: "programs",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                role = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "program_assignments",
            schema: "programs",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                program_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                type = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_program_assignments", x => x.id);
                table.ForeignKey(
                    name: "fk_program_assignments_programs_program_id",
                    column: x => x.program_id,
                    principalSchema: "programs",
                    principalTable: "programs",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_program_assignments_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "programs",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_program_assignments_program_id",
            schema: "programs",
            table: "program_assignments",
            column: "program_id");

        migrationBuilder.CreateIndex(
            name: "ix_program_assignments_program_id_user_id_type",
            schema: "programs",
            table: "program_assignments",
            columns: new[] { "program_id", "user_id", "type" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_program_assignments_user_id",
            schema: "programs",
            table: "program_assignments",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_programs_owner_id",
            schema: "programs",
            table: "programs",
            column: "owner_id");

        migrationBuilder.CreateIndex(
            name: "ix_users_email",
            schema: "programs",
            table: "users",
            column: "email",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "program_assignments",
            schema: "programs");

        migrationBuilder.DropTable(
            name: "programs",
            schema: "programs");

        migrationBuilder.DropTable(
            name: "users",
            schema: "programs");
    }
}
