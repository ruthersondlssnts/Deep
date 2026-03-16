using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deep.Programs.Application.Data.Migrations;

/// <inheritdoc />
public partial class EditProducts : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "pk_program_product",
            schema: "programs",
            table: "program_product"
        );

        migrationBuilder.AddColumn<string>(
            name: "cancellation_reason",
            schema: "programs",
            table: "programs",
            type: "character varying(500)",
            maxLength: 500,
            nullable: true
        );

        migrationBuilder.AddColumn<DateTime>(
            name: "cancelled_at_utc",
            schema: "programs",
            table: "programs",
            type: "timestamp with time zone",
            nullable: true
        );

        migrationBuilder.AddColumn<Guid>(
            name: "id",
            schema: "programs",
            table: "program_product",
            type: "uuid",
            nullable: false,
            defaultValue: Guid.Empty
        );

        migrationBuilder.AddColumn<int>(
            name: "reserved_stock",
            schema: "programs",
            table: "program_product",
            type: "integer",
            nullable: false,
            defaultValue: 0
        );

        migrationBuilder.AddColumn<string>(
            name: "sku",
            schema: "programs",
            table: "program_product",
            type: "character varying(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: ""
        );

        migrationBuilder.AddColumn<int>(
            name: "stock",
            schema: "programs",
            table: "program_product",
            type: "integer",
            nullable: false,
            defaultValue: 0
        );

        migrationBuilder.AddColumn<decimal>(
            name: "unit_price",
            schema: "programs",
            table: "program_product",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            defaultValue: 0m
        );

        migrationBuilder.AddPrimaryKey(
            name: "pk_program_product",
            schema: "programs",
            table: "program_product",
            column: "id"
        );

        migrationBuilder.CreateIndex(
            name: "ix_programs_program_status",
            schema: "programs",
            table: "programs",
            column: "program_status"
        );

        migrationBuilder.CreateIndex(
            name: "ix_program_product_program_id_sku",
            schema: "programs",
            table: "program_product",
            columns: new[] { "program_id", "sku" },
            unique: true
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_programs_program_status",
            schema: "programs",
            table: "programs"
        );

        migrationBuilder.DropPrimaryKey(
            name: "pk_program_product",
            schema: "programs",
            table: "program_product"
        );

        migrationBuilder.DropIndex(
            name: "ix_program_product_program_id_sku",
            schema: "programs",
            table: "program_product"
        );

        migrationBuilder.DropColumn(
            name: "cancellation_reason",
            schema: "programs",
            table: "programs"
        );

        migrationBuilder.DropColumn(
            name: "cancelled_at_utc",
            schema: "programs",
            table: "programs"
        );

        migrationBuilder.DropColumn(name: "id", schema: "programs", table: "program_product");

        migrationBuilder.DropColumn(
            name: "reserved_stock",
            schema: "programs",
            table: "program_product"
        );

        migrationBuilder.DropColumn(name: "sku", schema: "programs", table: "program_product");

        migrationBuilder.DropColumn(name: "stock", schema: "programs", table: "program_product");

        migrationBuilder.DropColumn(
            name: "unit_price",
            schema: "programs",
            table: "program_product"
        );

        migrationBuilder.AddPrimaryKey(
            name: "pk_program_product",
            schema: "programs",
            table: "program_product",
            columns: new[] { "program_id", "product_name" }
        );
    }
}
