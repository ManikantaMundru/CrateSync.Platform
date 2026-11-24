using CrateSync.Platform.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrateSync.Platform.Sales.Infrastructure.Persistence.Migrations;

[DbContext(typeof(SalesDbContext))]
[Migration("20261124203100_AddSalesOrderLineOriginCountry")]
public partial class AddSalesOrderLineOriginCountry : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "OriginCountry",
            schema: "sales",
            table: "SalesOrderLines",
            type: "nvarchar(2)",
            maxLength: 2,
            nullable: false,
            defaultValue: "ZZ");

        migrationBuilder.DropIndex(
            name: "IX_SalesOrderLines_ProductVarietyId",
            schema: "sales",
            table: "SalesOrderLines");

        migrationBuilder.CreateIndex(
            name: "IX_SalesOrderLines_ProductVarietyId_OriginCountry",
            schema: "sales",
            table: "SalesOrderLines",
            columns: new[] { "ProductVarietyId", "OriginCountry" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_SalesOrderLines_ProductVarietyId_OriginCountry",
            schema: "sales",
            table: "SalesOrderLines");

        migrationBuilder.CreateIndex(
            name: "IX_SalesOrderLines_ProductVarietyId",
            schema: "sales",
            table: "SalesOrderLines",
            column: "ProductVarietyId");

        migrationBuilder.DropColumn(
            name: "OriginCountry",
            schema: "sales",
            table: "SalesOrderLines");
    }
}
