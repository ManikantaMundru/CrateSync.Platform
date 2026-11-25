using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace CrateSync.Platform.Inventory.Infrastructure.Persistence.Migrations;

public partial class AddWarehouseTransfers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "WarehouseTransfers", schema: "inventory", columns: table => new
        {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            SourceWarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            DestinationWarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ProductVarietyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            OriginCountry = table.Column<string>(type: "nvarchar(2)", nullable: false, maxLength: 2),
            BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            BatchCode = table.Column<string>(type: "nvarchar(100)", nullable: true, maxLength: 100),
            PackWeightKg = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
            FruitsPerPack = table.Column<int>(type: "int", nullable: true),
            Quantity = table.Column<int>(type: "int", nullable: false),
            Status = table.Column<string>(type: "nvarchar(40)", nullable: false, maxLength: 40),
            TransportCompany = table.Column<string>(type: "nvarchar(150)", nullable: false, maxLength: 150),
            DriverName = table.Column<string>(type: "nvarchar(150)", nullable: false, maxLength: 150),
            DriverPhone = table.Column<string>(type: "nvarchar(50)", nullable: false, maxLength: 50),
            VehicleNumber = table.Column<string>(type: "nvarchar(50)", nullable: true, maxLength: 50),
            TransferReference = table.Column<string>(type: "nvarchar(100)", nullable: true, maxLength: 100),
            Remarks = table.Column<string>(type: "nvarchar(250)", nullable: true, maxLength: 250),
            CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            DispatchedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            ReceivedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            CancelledAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            Accepted = table.Column<int>(type: "int", nullable: true),
            Damaged = table.Column<int>(type: "int", nullable: true),
            Missing = table.Column<int>(type: "int", nullable: true),
            ReceivingRemarks = table.Column<string>(type: "nvarchar(500)", nullable: true, maxLength: 500)
        }, constraints: table => table.PrimaryKey("PK_WarehouseTransfers", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_WarehouseTransfers_TenantId_SourceWarehouseId", schema: "inventory", table: "WarehouseTransfers", columns: new[] { "TenantId", "SourceWarehouseId" });
        migrationBuilder.CreateIndex(name: "IX_WarehouseTransfers_TenantId_DestinationWarehouseId", schema: "inventory", table: "WarehouseTransfers", columns: new[] { "TenantId", "DestinationWarehouseId" });
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM inventory.WarehouseTransfers) THROW 50001, 'Transfer history exists; rollback would lose stock reconciliation.', 1;");
        migrationBuilder.DropTable(name: "WarehouseTransfers", schema: "inventory");
    }
}
