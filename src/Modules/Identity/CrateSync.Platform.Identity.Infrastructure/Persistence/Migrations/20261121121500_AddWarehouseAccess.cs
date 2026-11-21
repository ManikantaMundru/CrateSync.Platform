using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrateSync.Platform.Identity.Infrastructure.Persistence.Migrations;

[DbContext(typeof(CrateSyncIdentityDbContext))]
[Migration("20261121121500_AddWarehouseAccess")]
public partial class AddWarehouseAccess : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserWarehouseAssignments",
            schema: "identity",
            columns: table => new
            {
                Id = table.Column<Guid>(
                    type: "uniqueidentifier",
                    nullable: false),
                MembershipId = table.Column<Guid>(
                    type: "uniqueidentifier",
                    nullable: false),
                WarehouseId = table.Column<Guid>(
                    type: "uniqueidentifier",
                    nullable: false),
                Role = table.Column<int>(
                    type: "int",
                    nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(
                    type: "datetimeoffset",
                    nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(
                    type: "datetimeoffset",
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_UserWarehouseAssignments",
                    x => x.Id);

                table.ForeignKey(
                    name: "FK_UserWarehouseAssignments_UserTenantMemberships_MembershipId",
                    column: x => x.MembershipId,
                    principalSchema: "identity",
                    principalTable: "UserTenantMemberships",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserWarehouseAssignments_MembershipId_WarehouseId",
            schema: "identity",
            table: "UserWarehouseAssignments",
            columns: new[]
            {
                "MembershipId",
                "WarehouseId"
            },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserWarehouseAssignments_WarehouseId",
            schema: "identity",
            table: "UserWarehouseAssignments",
            column: "WarehouseId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "UserWarehouseAssignments",
            schema: "identity");
    }
}
