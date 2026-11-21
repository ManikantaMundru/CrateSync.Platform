using CrateSync.Platform.Identity.Infrastructure.Identity;
using CrateSync.Platform.Identity.Infrastructure.Persistence;
using CrateSync.Platform.IdentityServer.Services;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CrateSync.Platform.IdentityServer.Pages.Account;

[Authorize]
public sealed class SelectTenantModel(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    UserTenantMembershipStore membershipStore,
    SessionClaimBuilder sessionClaimBuilder,
    IIdentityServerInteractionService interaction)
    : PageModel
{
    [BindProperty]
    public Guid SelectedTenantId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IReadOnlyCollection<MembershipOption>
        Memberships { get; private set; } =
            Array.Empty<MembershipOption>();

    public async Task<IActionResult> OnGetAsync()
    {
        var user =
            await userManager.GetUserAsync(User);

        if (user is null || !user.IsActive)
            return Challenge();

        await LoadMembershipsAsync(user);

        if (Memberships.Count == 0)
        {
            await signInManager.SignOutAsync();
            return RedirectToPage(
                "/Account/Login",
                new
                {
                    returnUrl = ReturnUrl
                });
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user =
            await userManager.GetUserAsync(User);

        if (user is null || !user.IsActive)
            return Challenge();

        var membership =
            await membershipStore
                .GetActiveMembershipAsync(
                    user.Id,
                    SelectedTenantId,
                    HttpContext.RequestAborted);

        if (membership is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "The selected tenant is not available for this account.");

            await LoadMembershipsAsync(user);
            return Page();
        }

        var existingAuthentication =
            await HttpContext.AuthenticateAsync(
                IdentityConstants.ApplicationScheme);

        var isPersistent =
            existingAuthentication.Properties?
                .IsPersistent
            ?? false;

        await signInManager.SignOutAsync();

        await signInManager.SignInWithClaimsAsync(
            user,
            isPersistent,
            sessionClaimBuilder
                .BuildTenantSession(
                    user,
                    membership));

        return RedirectToSafeReturnUrl(ReturnUrl);
    }

    private async Task LoadMembershipsAsync(
        ApplicationUser user)
    {
        var memberships =
            await membershipStore
                .GetActiveMembershipsAsync(
                    user.Id,
                    HttpContext.RequestAborted);

        Memberships =
            memberships
                .Select(
                    x =>
                        new MembershipOption(
                            x.TenantId,
                            x.Roles
                                .Select(
                                    role =>
                                        role.Role.ToString())
                                .OrderBy(name => name)
                                .ToArray()))
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

    public sealed record MembershipOption(
        Guid TenantId,
        IReadOnlyCollection<string> Roles);
}
