using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence.Migrations;

public partial class SupportCountBasedPackaging : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(name: "PackSize", schema: "logistics", table: "InboundShipmentLines",
            type: "decimal(18,3)", nullable: true, oldClrType: typeof(decimal), oldType: "decimal(18,3)");
        migrationBuilder.AddColumn<int>(name: "FruitsPerPack", schema: "logistics", table: "InboundShipmentLines",
            type: "int", nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM logistics.InboundShipmentLines WHERE PackSize IS NULL) THROW 50001, 'Count-based packages cannot be converted automatically to kilograms.', 1;");
        migrationBuilder.DropColumn(name: "FruitsPerPack", schema: "logistics", table: "InboundShipmentLines");
        migrationBuilder.AlterColumn<decimal>(name: "PackSize", schema: "logistics", table: "InboundShipmentLines",
            type: "decimal(18,3)", nullable: false, oldClrType: typeof(decimal), oldType: "decimal(18,3)", oldNullable: true);
    }
}
