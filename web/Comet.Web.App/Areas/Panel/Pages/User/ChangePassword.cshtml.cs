using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Model.Helpers;
using Comet.Web.App.Services;
using Comet.Web.App.Services.Identity;
using Comet.Web.App.Services.Password;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Web.App.Areas.Panel.Pages.User
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly ApplicationDbContext mContext;
        private readonly UserManager mUserManager;
        private readonly LanguageService mLanguage;
        private readonly IEmailSender mEmailService;
        private readonly SignInManager mSignInManager;
        private readonly LogService<ChangePasswordModel> mLogService;

        public ChangePasswordModel(UserManager userManager,
            ApplicationDbContext context,
            LanguageService language,
            IEmailSender emailService,
            SignInManager signInManager, 
            LogService<ChangePasswordModel> logService)
        {
            mUserManager = userManager;
            mContext = context;
            mLanguage = language;
            mEmailService = emailService;
            mSignInManager = signInManager;
            mLogService = logService;
        }

        [BindProperty(SupportsGet = true)]
        public int AccountId { get; set; }

        public string Username { get; private set; }

        [BindProperty] public InputModel Input { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await mUserManager.GetUserAsync(User);

            if (user == null)
                return Forbid();

            var account = await mContext.GameAccounts.AsQueryable().FirstOrDefaultAsync(x => x.AccountID == AccountId);

            if (account == null)
                return RedirectToPage("./Accounts");

            if (user.Id != account.ParentId)
                return RedirectToPage("./Accounts");

            Username = account.Username;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                this.FillErrorModel(mLanguage);
                return Page();
            }

            var user = await mUserManager.GetUserAsync(User);

            if (user == null)
                return Forbid();

            var account = await mContext.GameAccounts.AsQueryable().FirstOrDefaultAsync(x => x.AccountID == AccountId);

            if (account == null)
                return RedirectToPage("./Accounts");

            if (user.Id != account.ParentId)
                return RedirectToPage("./Accounts");

            account.Salt = GameAccount.GenerateSalt();
            account.Password = GameAccount.HashPassword(Input.Password, account.Salt);
            await mContext.SaveAsync(account);

            await mEmailService.SendEmailAsync(user.Email, 
                mLanguage.GetString("ChangeGamePasswordEmailSubject"),
                mLanguage.GetString("ChangeGamePasswordEmailHtml", account.Username, DateTime.Now.ToString("U")));

            await mLogService.LogAsync($"User {user.UserName} has changed {account.Username}`s password", LogHistory.LogType.ChangeGamePassword);
            await mSignInManager.RefreshSignInAsync(user);
            return RedirectToPage("./Accounts");
        }

        public class InputModel
        {
            [Required]
            [StringLength(16, ErrorMessage = "GamePasswordMaxLength")]
            [MinLength(8, ErrorMessage = "GamePasswordMinLength")]
            [DataType(DataType.Password)]
            [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)[a-zA-Z\\d]{8,}$", ErrorMessage = "GamePasswordRegex")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "PasswordNotEqual")]
            public string ConfirmPassword { get; set; }
        }
    }
}
