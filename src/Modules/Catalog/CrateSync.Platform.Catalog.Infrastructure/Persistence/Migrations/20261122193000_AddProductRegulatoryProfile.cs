using CrateSync.Platform.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrateSync.Platform.Catalog.Infrastructure.Persistence.Migrations;

[DbContext(typeof(CatalogDbContext))]
[Migration("20261122193000_AddProductRegulatoryProfile")]
public partial class AddProductRegulatoryProfile : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CustomsTariffCode",
            schema: "catalog",
            table: "Products",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ScientificName",
            schema: "catalog",
            table: "Products",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "RegulatoryCategory",
            schema: "catalog",
            table: "Products",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DefaultUnitOfMeasure",
            schema: "catalog",
            table: "Products",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CustomsTariffCode",
            schema: "catalog",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "ScientificName",
            schema: "catalog",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "RegulatoryCategory",
            schema: "catalog",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "DefaultUnitOfMeasure",
            schema: "catalog",
            table: "Products");
    }
}
