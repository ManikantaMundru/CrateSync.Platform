using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CrateSync.Platform.IdentityServer.Pages.Account;

public sealed class LogoutModel(
    SignInManager<
        CrateSync.Platform.Identity.Infrastructure.Identity.ApplicationUser>
        signInManager,
    IIdentityServerInteractionService interaction)
    : PageModel
{
    public async Task<IActionResult> OnGetAsync(
        string? logoutId)
    {
        var context =
            string.IsNullOrWhiteSpace(logoutId)
                ? null
                : await interaction
                    .GetLogoutContextAsync(logoutId, HttpContext.RequestAborted);

        await signInManager.SignOutAsync();

        if (!string.IsNullOrWhiteSpace(
                context?.PostLogoutRedirectUri))
        {
            return Redirect(
                context.PostLogoutRedirectUri);
        }

        return Page();
    }
}
