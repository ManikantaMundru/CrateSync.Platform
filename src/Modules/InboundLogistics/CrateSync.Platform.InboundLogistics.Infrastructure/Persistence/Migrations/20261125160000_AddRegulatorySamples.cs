using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence.Migrations;

public partial class AddRegulatorySamples : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "RegulatorySamples", schema: "logistics",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ShipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ShipmentLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Authority = table.Column<int>(type: "int", nullable: false),
                Quantity = table.Column<int>(type: "int", nullable: false),
                ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                RecordedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RegulatorySamples", x => x.Id);
                table.ForeignKey(name: "FK_RegulatorySamples_InboundShipments_ShipmentId", column: x => x.ShipmentId, principalSchema: "logistics", principalTable: "InboundShipments", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateIndex("IX_RegulatorySamples_ShipmentId", "RegulatorySamples", "ShipmentId", schema: "logistics");
        migrationBuilder.CreateIndex("IX_RegulatorySamples_ShipmentLineId", "RegulatorySamples", "ShipmentLineId", schema: "logistics");
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM logistics.RegulatorySamples) THROW 50001, 'Regulatory sample history exists; rollback would lose data.', 1;");
        migrationBuilder.DropTable("RegulatorySamples", "logistics");
    }
}
