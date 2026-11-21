using CrateSync.Platform.Identity.Domain.Authorization;
using CrateSync.Platform.Identity.Infrastructure.Identity;
using CrateSync.Platform.IdentityServer.Services;
using CrateSync.Platform.Tenancy.Application.Abstractions;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CrateSync.Platform.IdentityServer.Pages.Account;

[Authorize]
public sealed class EnterTenantModel(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    SessionClaimBuilder sessionClaimBuilder,
    ITenantReadService tenantReadService,
    IIdentityServerInteractionService interaction)
    : PageModel
{
    [BindProperty]
    public Guid SelectedTenantId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IReadOnlyCollection<TenantOption> Tenants { get; private set; } =
        Array.Empty<TenantOption>();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null || !user.IsActive)
            return Challenge();

        if (user.PlatformRole != PlatformRole.SuperAdmin)
            return Forbid();

        await LoadTenantsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null || !user.IsActive)
            return Challenge();

        if (user.PlatformRole != PlatformRole.SuperAdmin)
            return Forbid();

        if (!await tenantReadService.IsActiveAsync(
                SelectedTenantId,
                HttpContext.RequestAborted))
        {
            ModelState.AddModelError(
                string.Empty,
                "The selected tenant is not active.");

            await LoadTenantsAsync();
            return Page();
        }

        var existingAuthentication =
            await HttpContext.AuthenticateAsync(
                IdentityConstants.ApplicationScheme);

        var isPersistent =
            existingAuthentication.Properties?.IsPersistent
            ?? false;

        await signInManager.SignOutAsync();

        await signInManager.SignInWithClaimsAsync(
            user,
            isPersistent,
            sessionClaimBuilder.BuildPlatformTenantSession(
                user,
                SelectedTenantId));

        return RedirectToSafeReturnUrl(ReturnUrl);
    }

    private async Task LoadTenantsAsync()
    {
        var tenants =
            await tenantReadService.SearchAsync(
                true,
                HttpContext.RequestAborted);

        Tenants = tenants
            .Select(x =>
                new TenantOption(
                    x.Id,
                    x.Name))
            .OrderBy(x => x.Name)
            .ToArray();
    }

    private IActionResult RedirectToSafeReturnUrl(
        string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) &&
            interaction.IsValidReturnUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) &&
            Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return Redirect("/");
    }

    public sealed record TenantOption(
        Guid TenantId,
        string Name);
}
