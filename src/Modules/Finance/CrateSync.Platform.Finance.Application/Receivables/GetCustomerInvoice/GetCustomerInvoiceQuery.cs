using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;

namespace CrateSync.Platform.Finance.Application.Receivables.GetCustomerInvoice;

public sealed record GetCustomerInvoiceQuery(Guid CustomerInvoiceId)
    : IQuery<Result<CustomerInvoiceResponse>>;
