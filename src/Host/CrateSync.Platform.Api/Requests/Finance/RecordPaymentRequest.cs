namespace CrateSync.Platform.Api.Requests.Finance;

public sealed record RecordPaymentRequest(
    decimal Amount,
    string Reference);
