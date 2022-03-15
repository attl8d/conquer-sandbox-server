using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Comet.Web.Areas.Profile.Models;
using Comet.Web.Data.Entities;
using Comet.Web.Model.Identity;
using Comet.Web.Services;
using Comet.Web.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Comet.Web.Areas.Profile.Pages.Settings
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager mUserManager;
        private readonly LanguageService mLanguage;
        private readonly SignInManager mSignInManager;

        public IndexModel(UserManager userManager,
                          LanguageService language, SignInManager signInManager)
        {
            mUserManager = userManager;
            mLanguage = language;
            mSignInManager = signInManager;
        }

        public ProfileModel Profile { get; private set; }

        [BindProperty] public InputModel Input { get; set; }

        private void Load()
        {
            Profile = Profile = (ProfileModel) HttpContext.Items["Profile"];
            if (Profile == null)
                return;

            Profile.LoadedByOwner = true;

            Input ??= new InputModel();
            Input.Locality = Profile.Locality;
            Input.Gender = Profile.Gender;
            Input.Pronoun = Profile.Pronoun;
            Input.DateOfBirth = Profile.DateOfBirth;
        }

        public async Task OnGetAsync()
        {
            Load();

            ApplicationUser user = await mUserManager.GetUserAsync(User);
            if (user == null)
            {
                Response.Redirect("/Error");
                return;
            }

            if (Profile == null || user.Id != Profile.Id)
            {
                Response.Redirect("/Error");
                return;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ApplicationUser user = await mUserManager.GetUserAsync(User);

            if (user == null)
                return LocalRedirect("/");

            if (!ModelState.IsValid)
            {
                Load();
                return Page();
            }

            Dictionary<string, string> claimsPairs = new Dictionary<string, string>
            {
                {ClaimTypes.DateOfBirth, Input.DateOfBirth?.ToString("s")},
                {ClaimTypes.Gender, Input.Gender},
                {CustomClaimTypes.Pronoum, Input.Pronoun},
                {ClaimTypes.Locality, Input.Locality}
            };

            foreach ((string claimType, string claimValue) in claimsPairs)
            {
                Claim temp = User.FindFirst(claimType);

                if (string.IsNullOrEmpty(claimValue))
                {
                    if (temp != null)
                        await mUserManager.RemoveClaimAsync(user, temp);
                    continue;
                }

                Claim now = new(claimType, claimValue);

                if (!string.IsNullOrEmpty(claimValue))
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
            [DataType(DataType.Date)] public DateTime? DateOfBirth { get; set; }

            public string Gender { get; set; }

            [DataType(DataType.Text)] public string Pronoun { get; set; }

            [DataType(DataType.Text)] public string Locality { get; set; }
        }
    }
}