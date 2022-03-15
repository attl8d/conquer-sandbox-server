using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Comet.Database.Entities;
using Comet.Web.Data;
using Comet.Web.Data.Entities;
using Comet.Web.Model.Game;
using Comet.Web.Model.Helpers;
using Comet.Web.Model.Identity;
using Comet.Web.Services;
using Comet.Web.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Comet.Web.Areas.Panel.Pages.User
{
    [Authorize]
    public class AccountsModel : PageModel
    {
        private readonly UserManager mUserManager;
        private readonly LanguageService mLanguage;
        private readonly ConquerGameService mGameService;

        public AccountsModel(UserManager userManager, 
                             LanguageService language,
                             ConquerGameService gameService)
        {
            mUserManager = userManager;
            mLanguage = language;
            mGameService = gameService;
        }

        [BindProperty] public InputModel Input { get; set; }

        public List<GameAccountModel> Accounts { get; private set; } = new();


        public async Task<IActionResult> OnGetAsync()
        {
            Accounts = await mUserManager.GetGameAccountsAsync(await mUserManager.GetUserAsync(User));
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                this.FillErrorModel(mLanguage);
                return Page();
            }

            ApplicationUser user = await mUserManager.GetUserAsync(User);
            BanReport bans = await mUserManager.GetBanInformationAsync(user);
            if (bans is {IsAccountBanned: true})
            {
                ModelState.AddModelError("BannedAccount", "CannotCreateGameAccountUserBanned");
                this.FillErrorModel(mLanguage);
                return Page();
            }

            int result = await mGameService.CreateAccountAsync(Input.Username, Input.Password);
            if (result == 1)
            {
                ModelState.AddModelError("DuplicateAccount", "GameAccountAlreadyExists");
                this.FillErrorModel(mLanguage);
                return Page();
            }
            if (result == 2)
            {
                ModelState.AddModelError("DbError", "DbError");
                this.FillErrorModel(mLanguage);
                return Page();
            }
            
            return RedirectToPage();
        }

        public class InputModel
        {
            [Required]
            [RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "GameUserNameRegex")]
            [DataType(DataType.Text)]
            [StringLength(16, ErrorMessage = "GameUserNameMaxLength")]
            [MinLength(4, ErrorMessage = "GameUserNameMinLength")]
            public string Username { get; set; }

            [Required]
            [StringLength(16, ErrorMessage = "GamePasswordMaxLength")]
            [DataType(DataType.Password)]
#if !DEBUG
            [MinLength(8, ErrorMessage = "GamePasswordMinLength")]            
            [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)[a-zA-Z\\d]{8,}$", ErrorMessage = "GamePasswordRegex")]
#endif
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "PasswordNotEqual")]
            public string ConfirmPassword { get; set; }
        }

    }
}
