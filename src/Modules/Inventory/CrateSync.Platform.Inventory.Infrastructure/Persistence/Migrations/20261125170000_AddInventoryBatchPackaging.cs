using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace CrateSync.Platform.Inventory.Infrastructure.Persistence.Migrations;

public partial class AddInventoryBatchPackaging : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_InventoryBalances_TenantId_WarehouseId_ProductVarietyId_OriginCountry", schema: "inventory", table: "InventoryBalances");
        migrationBuilder.AddColumn<Guid>(name: "BatchId", schema: "inventory", table: "InventoryBalances", type: "uniqueidentifier", nullable: false, defaultValue: Guid.Empty);
        migrationBuilder.AddColumn<string>(name: "BatchCode", schema: "inventory", table: "InventoryBalances", type: "nvarchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<decimal>(name: "PackWeightKg", schema: "inventory", table: "InventoryBalances", type: "decimal(18,3)", nullable: true);
        migrationBuilder.AddColumn<int>(name: "FruitsPerPack", schema: "inventory", table: "InventoryBalances", type: "int", nullable: true);
        migrationBuilder.CreateIndex(name: "IX_InventoryBalances_TenantId_WarehouseId_ProductVarietyId_OriginCountry_BatchId", schema: "inventory", table: "InventoryBalances", columns: new[] { "TenantId", "WarehouseId", "ProductVarietyId", "OriginCountry", "BatchId" }, unique: true);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM inventory.InventoryBalances WHERE BatchId <> '00000000-0000-0000-0000-000000000000') THROW 50001, 'Batch inventory exists; rollback would lose stock identity.', 1;");
        migrationBuilder.DropIndex(name: "IX_InventoryBalances_TenantId_WarehouseId_ProductVarietyId_OriginCountry_BatchId", schema: "inventory", table: "InventoryBalances");
        foreach (var name in new[] { "BatchId", "BatchCode", "PackWeightKg", "FruitsPerPack" })
            migrationBuilder.DropColumn(name: name, schema: "inventory", table: "InventoryBalances");
        migrationBuilder.CreateIndex(name: "IX_InventoryBalances_TenantId_WarehouseId_ProductVarietyId_OriginCountry", schema: "inventory", table: "InventoryBalances", columns: new[] { "TenantId", "WarehouseId", "ProductVarietyId", "OriginCountry" }, unique: true);
    }
}
