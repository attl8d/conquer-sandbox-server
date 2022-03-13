using System.Threading.Tasks;
using Comet.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Comet.Web.Pages
{
    [AllowAnonymous]
    public class ConfirmEmailChangeModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> mSignInManager;
        private readonly UserManager<ApplicationUser> mUserManager;

        public ConfirmEmailChangeModel(UserManager<ApplicationUser> userManager,
                                       SignInManager<ApplicationUser> signInManager)
        {
            mUserManager = userManager;
            mSignInManager = signInManager;
        }

        [TempData] public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string email, string code)
        {
            if (userId == null || email == null || code == null) return RedirectToPage("/Index");

            ApplicationUser user = await mUserManager.FindByIdAsync(userId);
            if (user == null) return NotFound($"Unable to load user with ID '{userId}'.");

            IdentityResult result = await mUserManager.ChangeEmailAsync(user, email, code);
            if (!result.Succeeded)
            {
                StatusMessage = "ErrorChangingEmail";
                return Page();
            }

            await mSignInManager.RefreshSignInAsync(user);
            StatusMessage = "EmailChangedSuccessfully";
            return Page();
        }
    }
}