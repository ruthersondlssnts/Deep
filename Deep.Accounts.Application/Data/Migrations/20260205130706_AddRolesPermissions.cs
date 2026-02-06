using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Deep.Accounts.Application.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "role",
                schema: "accounts",
                table: "accounts");

            migrationBuilder.CreateTable(
                name: "permissions",
                schema: "accounts",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissions", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "accounts",
                columns: table => new
                {
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "account_roles",
                schema: "accounts",
                columns: table => new
                {
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_name = table.Column<string>(type: "character varying(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_account_roles", x => new { x.account_id, x.role_name });
                    table.ForeignKey(
                        name: "fk_account_roles_accounts_account_id",
                        column: x => x.account_id,
                        principalSchema: "accounts",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_account_roles_roles_roles_name",
                        column: x => x.role_name,
                        principalSchema: "accounts",
                        principalTable: "roles",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "accounts",
                columns: table => new
                {
                    permission_code = table.Column<string>(type: "character varying(100)", nullable: false),
                    role_name = table.Column<string>(type: "character varying(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => new { x.permission_code, x.role_name });
                    table.ForeignKey(
                        name: "fk_role_permissions_permissions_permission_code",
                        column: x => x.permission_code,
                        principalSchema: "accounts",
                        principalTable: "permissions",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_permissions_roles_role_name",
                        column: x => x.role_name,
                        principalSchema: "accounts",
                        principalTable: "roles",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "accounts",
                table: "permissions",
                column: "code",
                values: new object[]
                {
                    "account.read.brandambassador",
                    "account.read.coordinator",
                    "account.read.itadmin",
                    "account.read.manager",
                    "account.read.programowner",
                    "account.register.brandambassador",
                    "account.register.coordinator",
                    "account.register.itadmin",
                    "account.register.manager",
                    "account.register.programowner",
                    "programs.assign.brandambassador",
                    "programs.assign.coordinator",
                    "programs.assign.coowner",
                    "programs.create",
                    "programs.read.all",
                    "programs.read.own",
                    "programs.update"
                });

            migrationBuilder.InsertData(
                schema: "accounts",
                table: "roles",
                column: "name",
                values: new object[]
                {
                    "BrandAmbassador",
                    "Coordinator",
                    "ItAdmin",
                    "Manager",
                    "ProgramOwner"
                });

            migrationBuilder.InsertData(
                schema: "accounts",
                table: "role_permissions",
                columns: new[] { "permission_code", "role_name" },
                values: new object[,]
                {
                    { "account.read.brandambassador", "Coordinator" },
                    { "account.read.coordinator", "ProgramOwner" },
                    { "account.read.itadmin", "ItAdmin" },
                    { "account.read.manager", "ItAdmin" },
                    { "account.read.programowner", "ItAdmin" },
                    { "account.register.brandambassador", "Coordinator" },
                    { "account.register.coordinator", "ProgramOwner" },
                    { "account.register.itadmin", "ItAdmin" },
                    { "account.register.manager", "ItAdmin" },
                    { "account.register.programowner", "ItAdmin" },
                    { "programs.assign.brandambassador", "Coordinator" },
                    { "programs.assign.coordinator", "ProgramOwner" },
                    { "programs.assign.coowner", "ProgramOwner" },
                    { "programs.create", "ProgramOwner" },
                    { "programs.read.all", "Manager" },
                    { "programs.read.own", "BrandAmbassador" },
                    { "programs.read.own", "ProgramOwner" },
                    { "programs.update", "ProgramOwner" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_account_roles_roles_name",
                schema: "accounts",
                table: "account_roles",
                column: "role_name");

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_role_name",
                schema: "accounts",
                table: "role_permissions",
                column: "role_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_roles",
                schema: "accounts");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "accounts");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "accounts");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "accounts");

            migrationBuilder.AddColumn<string>(
                name: "role",
                schema: "accounts",
                table: "accounts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
