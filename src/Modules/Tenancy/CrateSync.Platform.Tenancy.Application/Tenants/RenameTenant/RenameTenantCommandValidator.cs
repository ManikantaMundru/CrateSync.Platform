using FluentValidation;

namespace CrateSync.Platform.Tenancy.Application.Tenants.RenameTenant;

internal sealed class RenameTenantCommandValidator
    : AbstractValidator<RenameTenantCommand>
{
    public RenameTenantCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
