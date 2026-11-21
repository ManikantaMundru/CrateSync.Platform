using System.ComponentModel.DataAnnotations;
using CrateSync.Platform.Identity.Domain.Authorization;
using CrateSync.Platform.Identity.Infrastructure.Identity;
using CrateSync.Platform.Identity.Infrastructure.Persistence;
using CrateSync.Platform.IdentityServer.Services;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CrateSync.Platform.IdentityServer.Pages.Account;

public sealed class LoginModel(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    UserTenantMembershipStore membershipStore,
    SessionClaimBuilder sessionClaimBuilder,
    IIdentityServerInteractionService interaction)
    : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated == true &&
            !string.IsNullOrWhiteSpace(ReturnUrl))
        {
            return RedirectToSafeReturnUrl(ReturnUrl);
        }

        await Task.CompletedTask;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user =
            await FindUserAsync(Input.Login);

        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid username/email or password.");

            return Page();
        }

        var passwordResult =
            await signInManager.CheckPasswordSignInAsync(
                user,
                Input.Password,
                lockoutOnFailure: true);

        if (passwordResult.IsLockedOut)
        {
            ModelState.AddModelError(
                string.Empty,
                "This account is temporarily locked.");

            return Page();
        }

        if (passwordResult.IsNotAllowed)
        {
            ModelState.AddModelError(
                string.Empty,
                "Sign in is not currently allowed. Confirm your email or contact an administrator.");

            return Page();
        }

        if (!passwordResult.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid username/email or password.");

            return Page();
        }

        if (user.PlatformRole != PlatformRole.None)
        {
            await signInManager.SignInWithClaimsAsync(
                user,
                Input.RememberLogin,
                sessionClaimBuilder
                    .BuildPlatformSession(user));

            return RedirectToSafeReturnUrl(ReturnUrl);
        }

        var memberships =
            await membershipStore
                .GetActiveMembershipsAsync(
                    user.Id,
                    HttpContext.RequestAborted);

        if (memberships.Count == 0)
        {
            ModelState.AddModelError(
                string.Empty,
                "Your account is not assigned to an active tenant.");

            return Page();
        }

        if (memberships.Count == 1)
        {
            await signInManager.SignInWithClaimsAsync(
                user,
                Input.RememberLogin,
                sessionClaimBuilder
                    .BuildTenantSession(
                        user,
                        memberships[0]));

            return RedirectToSafeReturnUrl(ReturnUrl);
        }

        await signInManager.SignInWithClaimsAsync(
            user,
            Input.RememberLogin,
            sessionClaimBuilder
                .BuildTenantSelectionRequiredSession(
                    user));

        return RedirectToPage(
            "/Account/SelectTenant",
            new
            {
                returnUrl = ReturnUrl
            });
    }

    private async Task<ApplicationUser?>
        FindUserAsync(string login)
    {
        login = login.Trim();

        var byName =
            await userManager.FindByNameAsync(login);

        if (byName is not null)
            return byName;

        return await userManager.FindByEmailAsync(login);
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

    public sealed class InputModel
    {
        [Required]
        public string Login { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public bool RememberLogin { get; set; }
    }
}
