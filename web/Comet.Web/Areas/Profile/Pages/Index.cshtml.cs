using System.Security.Claims;
using System.Threading.Tasks;
using Comet.Web.Areas.Profile.Models;
using Comet.Web.Data.Entities;
using Comet.Web.Services.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Comet.Web.Areas.Profile.Pages
{
    public class IndexModel : PageModel
    {
        private readonly UserManager mUserManager;
        private readonly SignInManager mSignInManager;

        public IndexModel(UserManager userManager, 
                          SignInManager signInManager)
        {
            mUserManager = userManager;
            mSignInManager = signInManager;
        }

        [BindProperty(SupportsGet = true)]
        public string UserName { get; set; }

        public ProfileModel Profile { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ApplicationUser user;
            ClaimsPrincipal principal;
            if (string.IsNullOrWhiteSpace(UserName)) // self-profile
            {
                if (User.Identity?.IsAuthenticated != true)
                {
                    return LocalRedirect("/");
                }
                user = await mUserManager.GetUserAsync(User);
                principal = User;
            }
            else
            {
                user = await mUserManager.FindByNameAsync(UserName);
                principal = await mSignInManager.CreateUserPrincipalAsync(user);
            }

            if (user == null)
                return LocalRedirect("/");

            bool selfProfile = user.NormalizedUserName.Equals(mUserManager.NormalizeName(UserName));

            Profile = await mUserManager.GetUserProfileAsync(user, principal);
            Profile.LoadedByOwner = selfProfile;
            return Page();
        }
    }
}
