using System;
using System.Collections.Generic;
using System.Text;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.TradingPartners.Domain.Suppliers;

namespace CrateSync.Platform.TradingPartners.Application.Suppliers;

public static class SupplierErrors
{
    public static Error NotFound(SupplierId supplierId) =>
        Error.NotFound("TradingPartners.Supplier.NotFound", $"Supplier '{supplierId.Value}' was not found.");

    public static Error DuplicateCommercialIdentifier(string commercialIdentifier) =>
        Error.Conflict(
            "TradingPartners.Supplier.DuplicateCommercialIdentifier",
            $"A supplier with commercial identifier '{commercialIdentifier}' already exists.");
}
