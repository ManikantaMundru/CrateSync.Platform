using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace CrateSync.Platform.Inventory.Infrastructure.Persistence.Migrations;

public partial class AddWarehouseSellingPrices : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(name: "SellingPrice", schema: "inventory", table: "InventoryBalances", type: "decimal(18,2)", nullable: true);
        migrationBuilder.AddColumn<string>(name: "SellingCurrency", schema: "inventory", table: "InventoryBalances", type: "nvarchar(3)", maxLength: 3, nullable: true);
        migrationBuilder.AddColumn<DateTimeOffset>(name: "SellingPriceUpdatedAtUtc", schema: "inventory", table: "InventoryBalances", type: "datetimeoffset", nullable: true);
        migrationBuilder.CreateTable(name: "SellingPriceChanges", schema: "inventory", columns: table => new
        {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            InventoryBalanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            PreviousPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
            PreviousCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
            Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
            UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
            ChangedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_SellingPriceChanges", x => x.Id);
            table.ForeignKey(name: "FK_SellingPriceChanges_InventoryBalances_InventoryBalanceId", column: x => x.InventoryBalanceId, principalSchema: "inventory", principalTable: "InventoryBalances", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        });
        migrationBuilder.CreateIndex("IX_SellingPriceChanges_InventoryBalanceId", "SellingPriceChanges", "InventoryBalanceId", schema: "inventory");
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM inventory.SellingPriceChanges) THROW 50001, 'Selling price history exists; rollback would lose data.', 1;");
        migrationBuilder.DropTable("SellingPriceChanges", "inventory");
        foreach (var name in new[] { "SellingPrice", "SellingCurrency", "SellingPriceUpdatedAtUtc" })
            migrationBuilder.DropColumn(name: name, schema: "inventory", table: "InventoryBalances");
    }
}
