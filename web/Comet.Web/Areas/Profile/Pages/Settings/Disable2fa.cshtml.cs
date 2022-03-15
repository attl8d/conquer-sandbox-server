using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Comet.Web.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Comet.Web.Areas.Profile.Pages.Settings
{
    public class Disable2faModel : PageModel
    {
        private readonly UserManager<ApplicationUser> mUserManager;
        private readonly ILogger<Disable2faModel> mLogger;

        public Disable2faModel(
            UserManager<ApplicationUser> userManager,
            ILogger<Disable2faModel> logger)
        {
            mUserManager = userManager;
            mLogger = logger;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await mUserManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{mUserManager.GetUserId(User)}'.");
            }

            if (!await mUserManager.GetTwoFactorEnabledAsync(user))
            {
                throw new InvalidOperationException($"Cannot disable 2FA for user with ID '{mUserManager.GetUserId(User)}' as it's not currently enabled.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ApplicationUser user = await mUserManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{mUserManager.GetUserId(User)}'.");
            }

            var disable2FaResult = await mUserManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2FaResult.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred disabling 2FA for user with ID '{mUserManager.GetUserId(User)}'.");
            }

            Claim amr = (await mUserManager.GetClaimsAsync(user))?.FirstOrDefault(x => x.Type.Equals("amr"));
            if (amr != null)
                await mUserManager.RemoveClaimAsync(user, amr);

            mLogger.LogInformation("User with ID '{UserId}' has disabled 2fa.", mUserManager.GetUserId(User));
            StatusMessage = "2fa has been disabled. You can reenable 2fa when you setup an authenticator app";
            return RedirectToPage("./Security");
        }
    }
}
