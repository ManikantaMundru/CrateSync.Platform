using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence.Migrations;

public partial class AllowPendingShipmentPurchaseCosts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(
            name: "UnitCost", schema: "logistics", table: "InboundShipmentLines",
            type: "decimal(18,2)", nullable: true, oldClrType: typeof(decimal), oldType: "decimal(18,2)");
        migrationBuilder.AlterColumn<string>(
            name: "Currency", schema: "logistics", table: "InboundShipmentLines",
            type: "nvarchar(3)", maxLength: 3, nullable: true,
            oldClrType: typeof(string), oldType: "nvarchar(3)", oldMaxLength: 3);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Do not silently replace missing costs with fictitious values.
        migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM logistics.InboundShipmentLines WHERE UnitCost IS NULL OR Currency IS NULL) THROW 50001, 'Populate pending shipment purchase costs before rolling back.', 1;");
        migrationBuilder.AlterColumn<decimal>(
            name: "UnitCost", schema: "logistics", table: "InboundShipmentLines",
            type: "decimal(18,2)", nullable: false, oldClrType: typeof(decimal), oldType: "decimal(18,2)", oldNullable: true);
        migrationBuilder.AlterColumn<string>(
            name: "Currency", schema: "logistics", table: "InboundShipmentLines",
            type: "nvarchar(3)", maxLength: 3, nullable: false,
            oldClrType: typeof(string), oldType: "nvarchar(3)", oldMaxLength: 3, oldNullable: true);
    }
}
