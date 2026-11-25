using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence.Migrations;

public partial class AddWarehouseReceiptInspection : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var name in new[] { "AcceptedQuantity", "DamagedQuantity", "MissingQuantity" })
            migrationBuilder.AddColumn<int>(name: name, schema: "logistics", table: "ShipmentDeliveryAllocations", type: "int", nullable: true);
        migrationBuilder.AddColumn<string>(name: "ReceivingRemarks", schema: "logistics", table: "ShipmentDeliveryAllocations", type: "nvarchar(500)", maxLength: 500, nullable: true);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM logistics.ShipmentDeliveryAllocations WHERE AcceptedQuantity IS NOT NULL) THROW 50001, 'Receipt inspection history exists; rollback would lose data.', 1;");
        foreach (var name in new[] { "AcceptedQuantity", "DamagedQuantity", "MissingQuantity", "ReceivingRemarks" })
            migrationBuilder.DropColumn(name: name, schema: "logistics", table: "ShipmentDeliveryAllocations");
    }
}
