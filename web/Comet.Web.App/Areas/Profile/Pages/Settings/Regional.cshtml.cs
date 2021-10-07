using Comet.Web.App.Areas.Profile.Models;
using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Services;
using Comet.Web.App.Services.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Comet.Web.App.Areas.Profile.Pages.Settings
{
    public class RegionalModel : PageModel
    {
        private readonly UserManager mUserManager;
        private readonly ILogger<RegionalModel> mLogger;
        private readonly LanguageService mLanguage;
        private readonly ApplicationDbContext mDbContext;

        public RegionalModel(UserManager userManager,
            ILogger<RegionalModel> logger,
            LanguageService language,
            ApplicationDbContext dbContext)
        {
            mUserManager = userManager;
            mLogger = logger;
            mLanguage = language;
            mDbContext = dbContext;
        }

        public ProfileModel Profile { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ApplicationUser user = await mUserManager.GetUserAsync(User);

            if (user == null)
                return LocalRedirect("/");

            Profile = await mUserManager.GetUserProfileAsync(user);
            Profile.LoadedByOwner = true;
            Profile.CurrentPage = "SETTINGS";
            return Page();
        }
    }
}
