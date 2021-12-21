using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Model.Account;
using Comet.Web.App.Model.Helpers;
using Comet.Web.App.Services;
using Comet.Web.App.Services.Identity;
using Comet.Web.App.Services.Password;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Comet.Web.App.Areas.Panel.Pages.User
{
    [Authorize]
    public class AccountsModel : PageModel
    {
        private readonly UserManager mUserManager;
        private readonly ApplicationDbContext mContext;
        private readonly LanguageService mLanguage;
        private readonly LogService<AccountsModel> mLogService;

        public AccountsModel(UserManager userManager, ApplicationDbContext context, LanguageService language, LogService<AccountsModel> logService)
        {
            mUserManager = userManager;
            mContext = context;
            mLanguage = language;
            mLogService = logService;
        }

        [BindProperty] public InputModel Input { get; set; }

        public List<GameAccountModel> Accounts { get; private set; } = new List<GameAccountModel>();


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

            var currentUser = await mUserManager.GetUserAsync(User);
            var duplicate = await mContext.GameAccounts.FirstOrDefaultAsync(x => x.Username.Equals(Input.Username));
            if (duplicate != null)
            {
                ModelState.AddModelError("DuplicateAccount", "GameAccountAlreadyExists");
                this.FillErrorModel(mLanguage);
                return Page();
            }

            string salt = GameAccount.GenerateSalt();
            var account = new GameAccount
            {
                Username = Input.Username,
                Password = GameAccount.HashPassword(Input.Password, salt),
                IPAddress = "",
                AuthorityID = 1,
                ParentId = currentUser.Id,
                Registered = DateTime.Now,
                Salt = salt,
                MacAddress = ""
            };

            if (!await mContext.SaveAsync(account))
            {
                ModelState.AddModelError("DbError", "DbError");
                this.FillErrorModel(mLanguage);
                return Page();
            }

            await mLogService.LogAsync($"User [{currentUser.UserName}] has created game account [{account.Username}]", LogHistory.LogType.GameAccountCreation);
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
