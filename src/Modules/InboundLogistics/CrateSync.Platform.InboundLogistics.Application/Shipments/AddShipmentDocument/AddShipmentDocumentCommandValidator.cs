using FluentValidation;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.AddShipmentDocument;

internal sealed class AddShipmentDocumentCommandValidator
    : AbstractValidator<AddShipmentDocumentCommand>
{
    public AddShipmentDocumentCommandValidator()
    {
        RuleFor(x => x.ShipmentId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.DocumentName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ReferenceNumber).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
