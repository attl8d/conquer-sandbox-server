using System.Threading.Tasks;
using Comet.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Comet.Web.Pages
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> mLogger;
        private readonly SignInManager<ApplicationUser> mSignInManager;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
        {
            mSignInManager = signInManager;
            mLogger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await mSignInManager.SignOutAsync();

            if (returnUrl != null)
                return LocalRedirect(returnUrl);
            return RedirectToPage();
        }
    }
}