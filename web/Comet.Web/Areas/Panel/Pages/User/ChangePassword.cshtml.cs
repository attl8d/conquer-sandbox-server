using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Comet.Database.Entities;
using Comet.Web.Data;
using Comet.Web.Data.Entities;
using Comet.Web.Model.Helpers;
using Comet.Web.Services;
using Comet.Web.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Comet.Web.Areas.Panel.Pages.User
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly ApplicationDbContext mContext;
        private readonly UserManager mUserManager;
        private readonly LanguageService mLanguage;
        private readonly ConquerGameService mGameService;

        public ChangePasswordModel(UserManager userManager,
                                   ApplicationDbContext context,
                                   LanguageService language,
                                   ConquerGameService gameService)
        {
            mUserManager = userManager;
            mContext = context;
            mLanguage = language;
            mGameService = gameService;
        }

        [BindProperty(SupportsGet = true)] public int AccountId { get; set; }

        public string Username { get; private set; }

        [BindProperty] public InputModel Input { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ApplicationUser user = await mUserManager.GetUserAsync(User);

            if (user == null)
                return Forbid();

            DbAccount account =
                await mContext.GameAccounts.AsQueryable().FirstOrDefaultAsync(x => x.AccountID == AccountId);

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

            await mGameService.ChangePasswordAsync(AccountId, Input.Password);
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