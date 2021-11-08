using Comet.Web.App.Areas.Profile.Models;
using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Services;
using Comet.Web.App.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Comet.Web.App.Areas.Profile.Pages.Settings
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager mUserManager;
        private readonly ILogger<IndexModel> mLogger;
        private readonly LanguageService mLanguage;
        private readonly ApplicationDbContext mDbContext;
        private readonly SignInManager mSignInManager;

        public IndexModel(UserManager userManager,
            ILogger<IndexModel> logger,
            LanguageService language,
            ApplicationDbContext dbContext,
            SignInManager signInManager)
        {
            mUserManager = userManager;
            mLogger = logger;
            mLanguage = language;
            mDbContext = dbContext;
            mSignInManager = signInManager;
        }

        public ProfileModel Profile { get; private set; }

        [BindProperty] public InputModel Input { get; set; }

        private async Task LoadAsync()
        {
            Profile = await mUserManager.GetUserProfileAsync(await mUserManager.GetUserAsync(User));
            Profile.LoadedByOwner = true;
            Profile.CurrentPage = "SETTINGS";

            Input ??= new InputModel();
            Input.Locality = User.FindFirst(PersonalInfoType.Locality)?.Value;
            Input.Gender = User.FindFirst(PersonalInfoType.Gender)?.Value;
            Input.Pronoun = User.FindFirst(PersonalInfoType.Pronoun)?.Value;
            string dob = User.FindFirst(PersonalInfoType.DateOfBirth)?.Value;
            if (!string.IsNullOrEmpty(dob))
            {
                Input.DateOfBirth = DateTime.Parse(dob);
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            ApplicationUser user = await mUserManager.GetUserAsync(User);

            if (user == null)
                return LocalRedirect("/");

            await LoadAsync();            
            return Page();
        }

        public async Task<IActionResult> OnPostAsync() 
        {
            ApplicationUser user = await mUserManager.GetUserAsync(User);
            
            if (user == null)
                return LocalRedirect("/");

            if (!ModelState.IsValid)
            {
                await LoadAsync();
                return Page();
            }

            Dictionary<string, string> claimsPairs = new Dictionary<string, string>();
            claimsPairs.Add(PersonalInfoType.DateOfBirth.ToClaimString(), Input.DateOfBirth?.ToString("s"));
            claimsPairs.Add(PersonalInfoType.Gender.ToClaimString(), Input.Gender);
            claimsPairs.Add(PersonalInfoType.Pronoun.ToClaimString(), Input.Pronoun);
            claimsPairs.Add(PersonalInfoType.Locality.ToClaimString(), Input.Locality);

            foreach (var claim in claimsPairs)
            {
                Claim temp = User.FindFirst(claim.Key);

                if (string.IsNullOrEmpty(claim.Value))
                {
                    if (temp != null)
                        await mUserManager.RemoveClaimAsync(user, temp);
                    continue;
                }

                Claim now = new (claim.Key, claim.Value);
                
                if (!string.IsNullOrEmpty(claim.Value))
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
            }

            await mSignInManager.RefreshSignInAsync(user);
            return RedirectToPage();
        }

        public class InputModel
        {
            [DataType(DataType.Date)]
            public DateTime? DateOfBirth { get; set; }

            public string Gender { get; set; }

            [DataType(DataType.Text)]
            public string Pronoun { get; set; }

            [DataType(DataType.Text)]
            public string Locality { get; set; }
        }
    }
}
