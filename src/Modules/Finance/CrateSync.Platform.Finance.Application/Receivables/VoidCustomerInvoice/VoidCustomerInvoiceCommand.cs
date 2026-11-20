using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Finance.Application.Receivables.VoidCustomerInvoice;

public sealed record VoidCustomerInvoiceCommand(
    Guid CustomerInvoiceId) : ICommand<Result>;
