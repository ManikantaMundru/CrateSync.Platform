using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace CrateSync.Platform.Sales.Infrastructure.Persistence.Migrations;
public partial class AddSalesBatchSnapshots : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(name: "BatchId", schema: "sales", table: "SalesOrderLines", type: "uniqueidentifier", nullable: false, defaultValue: Guid.Empty);
        migrationBuilder.AddColumn<string>(name: "BatchCode", schema: "sales", table: "SalesOrderLines", type: "nvarchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<int>(name: "FruitsPerPack", schema: "sales", table: "SalesOrderLines", type: "int", nullable: true);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sales.SalesOrderLines WHERE BatchId <> '00000000-0000-0000-0000-000000000000') THROW 50001, 'Batch sales history exists; rollback would lose traceability.', 1;");
        foreach (var name in new[] { "BatchId", "BatchCode", "FruitsPerPack" })
            migrationBuilder.DropColumn(name: name, schema: "sales", table: "SalesOrderLines");
    }
}
