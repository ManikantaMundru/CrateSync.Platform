using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Finance.Application.Payables.RecordSupplierPayment;

public sealed record RecordSupplierPaymentCommand(
    Guid SupplierInvoiceId,
    decimal Amount,
    string Reference) : ICommand<Result<Guid>>;
