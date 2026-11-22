using FluentValidation;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.UpdateShipmentReferences;

internal sealed class UpdateShipmentReferencesCommandValidator
    : AbstractValidator<UpdateShipmentReferencesCommand>
{
    public UpdateShipmentReferencesCommandValidator()
    {
        RuleFor(x => x.ShipmentId).NotEmpty();
        RuleFor(x => x.ContainerNumber).MaximumLength(50);
        RuleFor(x => x.BillOfLadingNumber).MaximumLength(100);
        RuleFor(x => x.PortOfLoading).MaximumLength(150);
        RuleFor(x => x.PortOfDischarge).MaximumLength(150);
        RuleFor(x => x.VesselName).MaximumLength(150);
        RuleFor(x => x.VoyageNumber).MaximumLength(100);
        RuleFor(x => x.CustomsBrokerName).MaximumLength(150);
        RuleFor(x => x.BillOfEntryNumber).MaximumLength(100);
    }
}
