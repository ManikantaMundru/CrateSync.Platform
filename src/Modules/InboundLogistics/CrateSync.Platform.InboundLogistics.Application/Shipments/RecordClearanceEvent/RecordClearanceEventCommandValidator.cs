using FluentValidation;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.RecordClearanceEvent;

internal sealed class RecordClearanceEventCommandValidator
    : AbstractValidator<RecordClearanceEventCommand>
{
    public RecordClearanceEventCommandValidator()
    {
        RuleFor(x => x.ShipmentId).NotEmpty();
        RuleFor(x => x.Authority).IsInEnum();
        RuleFor(x => x.Status).NotEmpty().MaximumLength(80);
        RuleFor(x => x.ReferenceNumber).MaximumLength(100);
        RuleFor(x => x.Remarks).MaximumLength(1000);
    }
}
