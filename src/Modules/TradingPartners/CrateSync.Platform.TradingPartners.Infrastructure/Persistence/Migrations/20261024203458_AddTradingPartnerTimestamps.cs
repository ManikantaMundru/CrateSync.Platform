using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrateSync.Platform.TradingPartners.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTradingPartnerTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                schema: "partners",
                table: "Suppliers",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeactivatedAtUtc",
                schema: "partners",
                table: "Suppliers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                schema: "partners",
                table: "Suppliers",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                schema: "partners",
                table: "Customers",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeactivatedAtUtc",
                schema: "partners",
                table: "Customers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAtUtc",
                schema: "partners",
                table: "Customers",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "partners",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "DeactivatedAtUtc",
                schema: "partners",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "partners",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "partners",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DeactivatedAtUtc",
                schema: "partners",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "partners",
                table: "Customers");
        }
    }
}
