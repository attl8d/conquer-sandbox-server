using System.Text;
using System.Threading.Tasks;
using Comet.Web.Data.Entities;
using Comet.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Comet.Web.Pages
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly LanguageService mLanguage;
        private readonly UserManager<ApplicationUser> mUserManager;

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager, LanguageService language)
        {
            mUserManager = userManager;
            mLanguage = language;
        }

        [TempData] public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null) return RedirectToPage("/Index");

            ApplicationUser user = await mUserManager.FindByIdAsync(userId);
            if (user == null) return NotFound(mLanguage.GetString("CouldNotFindAccountToActivate"));

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            IdentityResult result = await mUserManager.ConfirmEmailAsync(user, code);
            StatusMessage = result.Succeeded ? "AccountActivatedSuccessfully" : "CouldNotActivateAccount";
            return Page();
        }
    }
}