using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Finance.Application.Payables.VoidSupplierInvoice;

public sealed record VoidSupplierInvoiceCommand(
    Guid SupplierInvoiceId) : ICommand<Result>;
