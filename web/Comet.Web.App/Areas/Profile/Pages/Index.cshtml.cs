using Comet.Web.App.Areas.Profile.Models;
using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Services;
using Comet.Web.App.Services.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Comet.Web.App.Areas.Profile.Pages
{
    public class IndexModel : PageModel
    {
        private readonly UserManager mUserManager;
        private readonly ILogger<IndexModel> mLogger;
        private readonly LanguageService mLanguage;
        private readonly ApplicationDbContext mDbContext;

        public IndexModel(UserManager userManager,
            ILogger<IndexModel> logger,
            LanguageService language,
            ApplicationDbContext dbContext)
        {
            mUserManager = userManager;
            mLogger = logger;
            mLanguage = language;
            mDbContext = dbContext;
        }

        [BindProperty(SupportsGet = true)]
        public string UserName { get; set; }

        public ProfileModel Profile { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ApplicationUser user;
            if (string.IsNullOrWhiteSpace(UserName)) // self-profile
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return LocalRedirect("/");
                }
                user = await mUserManager.GetUserAsync(User);
            }
            else
            {
                user = await mUserManager.FindByNameAsync(UserName);
            }

            if (user == null)
                return LocalRedirect("/");

            bool selfProfile = user.NormalizedUserName.Equals(mUserManager.NormalizeName(UserName));

            Profile = await mUserManager.GetUserProfileAsync(user);
            Profile.LoadedByOwner = selfProfile;
            Profile.CurrentPage = "INDEX";
            return Page();
        }
    }
}
