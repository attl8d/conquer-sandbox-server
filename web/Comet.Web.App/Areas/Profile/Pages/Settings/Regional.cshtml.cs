using Comet.Web.App.Areas.Profile.Models;
using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Services;
using Comet.Web.App.Services.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
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

        [BindProperty] public InputModel Input { get; set; }

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

        public async Task<IActionResult> OnPostAsync()
        {
            ApplicationUser user = await mUserManager.GetUserAsync(User);
            if (user == null)
                return LocalRedirect("/");

            Claim temp = User.FindFirst(PersonalInfoType.Language);
            Claim now = new(PersonalInfoType.Language.ToClaimString(), Input.Language);
            if (!string.IsNullOrEmpty(Input.Language))
            {
                if (temp != null)
                {
                    await mUserManager.ReplaceClaimAsync(user, temp, now);
                }
                else
                {
                    await mUserManager.AddClaimAsync(user, now);
                }
            }

            return RedirectToPage();
        }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            public string Language { get; set; }
        }
    }
}
