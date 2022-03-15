using System.Threading.Tasks;
using Comet.Web.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Comet.Web.Areas.Profile.Pages.Settings
{
    public class ResetAuthenticatorModel : PageModel
    {
        private UserManager<ApplicationUser> mUserManager;
        private readonly SignInManager<ApplicationUser> mSignInManager;
        private ILogger<ResetAuthenticatorModel> mLogger;

        public ResetAuthenticatorModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ResetAuthenticatorModel> logger)
        {
            mUserManager = userManager;
            mSignInManager = signInManager;
            mLogger = logger;
        }

        [TempData] public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ApplicationUser user = await mUserManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{user.Id}'.");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ApplicationUser user = await mUserManager.GetUserAsync(User);
            if (user == null) return NotFound("Unable to load user with ID 'user.Id'.");

            await mUserManager.SetTwoFactorEnabledAsync(user, false);
            await mUserManager.ResetAuthenticatorKeyAsync(user);
            mLogger.LogInformation("User with ID '{UserId}' has reset their authentication app key.", user.Id);

            await mSignInManager.RefreshSignInAsync(user);
            StatusMessage =
                "Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.";

            return RedirectToPage("./EnableAuthenticator");
        }
    }
}