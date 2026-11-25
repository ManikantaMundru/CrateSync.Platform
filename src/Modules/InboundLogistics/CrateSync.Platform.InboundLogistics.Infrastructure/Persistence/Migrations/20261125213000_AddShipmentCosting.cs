using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence.Migrations;
public partial class AddShipmentCosting : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "CostingCurrency", schema: "logistics", table: "InboundShipments", type: "nvarchar(3)", maxLength: 3, nullable: true);
        migrationBuilder.CreateTable(name: "ShipmentExpenses", schema: "logistics", columns: table => new
        {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ShipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Category = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
            Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
            ExchangeRate = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
            DeliveryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            LineId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
            CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            ReversedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            ReversedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            ReversalReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_ShipmentExpenses", x => x.Id);
            table.ForeignKey(name: "FK_ShipmentExpenses_InboundShipments_ShipmentId", column: x => x.ShipmentId, principalSchema: "logistics", principalTable: "InboundShipments", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        });
        migrationBuilder.CreateIndex(name: "IX_ShipmentExpenses_ShipmentId", schema: "logistics", table: "ShipmentExpenses", column: "ShipmentId");
        migrationBuilder.CreateTable(name: "ShipmentCostExchangeRates", schema: "logistics", columns: table => new
        {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ShipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
            Rate = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
            UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_ShipmentCostExchangeRates", x => x.Id);
            table.ForeignKey(name: "FK_ShipmentCostExchangeRates_InboundShipments_ShipmentId", column: x => x.ShipmentId, principalSchema: "logistics", principalTable: "InboundShipments", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        });
        migrationBuilder.CreateIndex(name: "IX_ShipmentCostExchangeRates_ShipmentId_Currency", schema: "logistics", table: "ShipmentCostExchangeRates", columns: new[] { "ShipmentId", "Currency" }, unique: true);

    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM logistics.ShipmentExpenses) OR EXISTS (SELECT 1 FROM logistics.ShipmentCostExchangeRates) THROW 50001, 'Costing history exists; rollback would lose data.', 1;");
        migrationBuilder.DropTable("ShipmentExpenses", "logistics");
        migrationBuilder.DropTable("ShipmentCostExchangeRates", "logistics");
        migrationBuilder.DropColumn(name: "CostingCurrency", schema: "logistics", table: "InboundShipments");
    }
}
