using System.Threading.Tasks;
using Comet.Web.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Comet.Web.Areas.Profile.Pages.Settings
{
    public class TwoFactorAuthenticationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> mUserManager;
        private readonly SignInManager<ApplicationUser> mSignInManager;

        public TwoFactorAuthenticationModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<TwoFactorAuthenticationModel> logger)
        {
            mUserManager = userManager;
            mSignInManager = signInManager;
        }

        public bool HasAuthenticator { get; set; }

        public int RecoveryCodesLeft { get; set; }

        [BindProperty] public bool Is2faEnabled { get; set; }

        public bool IsMachineRemembered { get; set; }

        [TempData] public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ApplicationUser user = await mUserManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{mUserManager.GetUserId(User)}'.");

            HasAuthenticator = await mUserManager.GetAuthenticatorKeyAsync(user) != null;
            Is2faEnabled = await mUserManager.GetTwoFactorEnabledAsync(user);
            IsMachineRemembered = await mSignInManager.IsTwoFactorClientRememberedAsync(user);
            RecoveryCodesLeft = await mUserManager.CountRecoveryCodesAsync(user);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ApplicationUser user = await mUserManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{mUserManager.GetUserId(User)}'.");

            await mSignInManager.ForgetTwoFactorClientAsync();
            StatusMessage =
                "The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.";
            return RedirectToPage();
        }
    }
}