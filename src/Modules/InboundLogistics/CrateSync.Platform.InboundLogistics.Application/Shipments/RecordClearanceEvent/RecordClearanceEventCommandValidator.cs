using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using FluentValidation;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.RecordClearanceEvent;

internal sealed class RecordClearanceEventCommandValidator
    : AbstractValidator<RecordClearanceEventCommand>
{
    public RecordClearanceEventCommandValidator()
    {
        RuleFor(x => x.ShipmentId).NotEmpty();
        RuleFor(x => x.Authority).IsInEnum();

        RuleFor(x => x.Status)
            .NotEmpty()
            .MaximumLength(80)
            .Must((command, status) =>
                IsValidStatus(command.Authority, status))
            .WithMessage(
                "Status is not valid for the selected clearance authority.");

        RuleFor(x => x.ReferenceNumber).MaximumLength(100);
        RuleFor(x => x.Remarks).MaximumLength(1000);
    }

    private static bool IsValidStatus(
        ClearanceAuthority authority,
        string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return false;

        return authority switch
        {
            ClearanceAuthority.Customs =>
                Enum.TryParse<CustomsClearanceStatus>(
                    status,
                    true,
                    out _),

            ClearanceAuthority.PlantQuarantine =>
                Enum.TryParse<PlantQuarantineStatus>(
                    status,
                    true,
                    out _),

            ClearanceAuthority.FoodSafety =>
                Enum.TryParse<FoodSafetyStatus>(
                    status,
                    true,
                    out _),

            ClearanceAuthority.Port or
            ClearanceAuthority.Other => true,

            _ => false
        };
    }
}
