using System.ComponentModel.DataAnnotations;
using System.Text;
using CrateSync.Platform.Identity.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace CrateSync.Platform.IdentityServer.Pages.Account;

public sealed class ActivateModel(
    UserManager<ApplicationUser> userManager,
    TimeProvider timeProvider,
    IConfiguration configuration)
    : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid UserId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Token { get; set; } = string.Empty;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool Completed { get; private set; }

    public string WebBaseUrl =>
        configuration[
            "Clients:CrateSyncWeb:BaseUrl"]
        ?? "http://localhost:5173";

    public async Task<IActionResult> OnGetAsync()
    {
        if (UserId == Guid.Empty ||
            string.IsNullOrWhiteSpace(Token))
        {
            return BadRequest();
        }

        var user =
            await userManager.FindByIdAsync(
                UserId.ToString());

        if (user is null || !user.IsActive)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user =
            await userManager.FindByIdAsync(
                UserId.ToString());

        if (user is null || !user.IsActive)
            return NotFound();

        string decodedToken;

        try
        {
            decodedToken =
                Encoding.UTF8.GetString(
                    WebEncoders.Base64UrlDecode(
                        Token));
        }
        catch (FormatException)
        {
            ModelState.AddModelError(
                string.Empty,
                "This activation link is invalid.");

            return Page();
        }

        var resetResult =
            await userManager.ResetPasswordAsync(
                user,
                decodedToken,
                Input.Password);

        if (!resetResult.Succeeded)
        {
            foreach (var error in resetResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return Page();
        }

        user.EmailConfirmed = true;
        user.UpdatedAtUtc =
            timeProvider.GetUtcNow();

        var updateResult =
            await userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return Page();
        }

        Completed = true;
        return Page();
    }

    public sealed class InputModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } =
            string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(
            nameof(Password),
            ErrorMessage =
                "Passwords do not match.")]
        public string ConfirmPassword { get; set; } =
            string.Empty;
    }
}
