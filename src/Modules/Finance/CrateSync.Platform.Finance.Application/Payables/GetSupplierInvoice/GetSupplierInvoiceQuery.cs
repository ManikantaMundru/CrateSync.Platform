using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;

namespace CrateSync.Platform.Finance.Application.Payables.GetSupplierInvoice;

public sealed record GetSupplierInvoiceQuery(Guid SupplierInvoiceId)
    : IQuery<Result<SupplierInvoiceResponse>>;
