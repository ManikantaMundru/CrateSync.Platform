using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrateSync.Platform.Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inventory");

            migrationBuilder.CreateTable(
                name: "InventoryBalances",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductVarietyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OnHand = table.Column<int>(type: "int", nullable: false),
                    Reserved = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryBalances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockMovements",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    OnHandChange = table.Column<int>(type: "int", nullable: false),
                    ReservedChange = table.Column<int>(type: "int", nullable: false),
                    OnHandAfter = table.Column<int>(type: "int", nullable: false),
                    ReservedAfter = table.Column<int>(type: "int", nullable: false),
                    ReferenceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    InventoryBalanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockMovements_InventoryBalances_InventoryBalanceId",
                        column: x => x.InventoryBalanceId,
                        principalSchema: "inventory",
                        principalTable: "InventoryBalances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBalances_TenantId_ProductVarietyId",
                schema: "inventory",
                table: "InventoryBalances",
                columns: new[] { "TenantId", "ProductVarietyId" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBalances_TenantId_WarehouseId",
                schema: "inventory",
                table: "InventoryBalances",
                columns: new[] { "TenantId", "WarehouseId" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBalances_TenantId_WarehouseId_ProductVarietyId",
                schema: "inventory",
                table: "InventoryBalances",
                columns: new[] { "TenantId", "WarehouseId", "ProductVarietyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_InventoryBalanceId",
                schema: "inventory",
                table: "StockMovements",
                column: "InventoryBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_OccurredAtUtc",
                schema: "inventory",
                table: "StockMovements",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ReferenceType_ReferenceId",
                schema: "inventory",
                table: "StockMovements",
                columns: new[] { "ReferenceType", "ReferenceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockMovements",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "InventoryBalances",
                schema: "inventory");
        }
    }
}
