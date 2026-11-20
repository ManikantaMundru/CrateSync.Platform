using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Finance.Application.Receivables.RecordCustomerPayment;

public sealed record RecordCustomerPaymentCommand(
    Guid CustomerInvoiceId,
    decimal Amount,
    string Reference) : ICommand<Result<Guid>>;
