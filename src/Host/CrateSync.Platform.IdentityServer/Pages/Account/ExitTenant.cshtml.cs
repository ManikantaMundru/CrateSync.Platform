using CrateSync.Platform.Identity.Infrastructure.Identity;
using CrateSync.Platform.IdentityServer.Services;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CrateSync.Platform.IdentityServer.Pages.Account;

[Authorize]
public sealed class ExitTenantModel(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    SessionClaimBuilder sessionClaimBuilder,
    IIdentityServerInteractionService interaction)
    : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null || !user.IsActive)
            return Challenge();

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
            sessionClaimBuilder.BuildPlatformSession(user));

        return RedirectToSafeReturnUrl(ReturnUrl);
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
}
