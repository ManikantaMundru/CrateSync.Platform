using FluentValidation;

namespace CrateSync.Platform.Tenancy.Application.Tenants.CreateTenant;

internal sealed class CreateTenantCommandValidator
    : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}
