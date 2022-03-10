using Comet.Web.Areas.Profile.Models;
using Comet.Web.Data.Entities;
using Comet.Web.Model.Identity;
using Comet.Web.Services;
using Comet.Web.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static Comet.Web.Services.Security;

namespace Comet.Web.Areas.Panel.Pages.Administration
{
    [Authorize(Policy = Moderator)]
    public class UserModel : PageModel
    {
        private readonly RoleManager mRoleManager;
        private readonly UserManager mUserManager;
        private readonly SignInManager mSigninManager;
        private readonly LanguageService mLanguage;
        private readonly IEmailSender mEmailSender;

        public UserModel(UserManager userManager,
            SignInManager signInManager,
            LanguageService languageService,
            IEmailSender emailService, RoleManager roleManager)
        {
            mUserManager = userManager;
            mSigninManager = signInManager;
            mLanguage = languageService;
            mEmailSender = emailService;
            mRoleManager = roleManager;
        }

        public Dictionary<string, Claims> Claims = new Dictionary<string, Claims>();

        [BindProperty(SupportsGet = true)] public int UserId { get; set; }

        public ApplicationUser Account { get; set; }
        public ClaimsPrincipal AccountPrincipal { get; set; }
        public BanReport BanReport { get; set; }

        [BindProperty] public UserInputModel InputUser { get; set; }
        [BindProperty] public UserBanModel InputBan { get; set; }

        private async Task LoadAsync()
        {
            Account = await mUserManager.FindByIdAsync(UserId.ToString());
            if (Account == null)
                return;

            Claims = new Dictionary<string, Claims>(Enum.GetValues<Claims>().Where(x => x != Security.Claims.Super).ToDictionary(x => x.ToString()));

            AccountPrincipal = await mSigninManager.CreateUserPrincipalAsync(Account);
            BanReport = await mUserManager.GetBanInformationAsync(Account);

            ProfileModel profile = await mUserManager.GetUserProfileAsync(Account, AccountPrincipal);
            InputUser ??= new UserInputModel()
            {
                Email = Account.Email,
                UserName = Account.UserName,
                TwoFactorEnabled = Account.TwoFactorEnabled
            };

            if (profile.DateOfBirth.HasValue)
            {
                InputUser.BirthDate = profile.DateOfBirth.Value;
            }

            if (!string.IsNullOrEmpty(profile.Gender))
            {
                InputUser.Gender = profile.Gender;
            }

            if (!string.IsNullOrEmpty(profile.Locality))
            {
                InputUser.Locality = profile.Locality;
            }

            if (!string.IsNullOrEmpty(profile.Pronoun))
            {
                InputUser.Pronoun = profile.Pronoun;
            }

            if (!string.IsNullOrEmpty(profile.SocialName))
            {
                InputUser.Pronoun = profile.SocialName;
            }

            if (!string.IsNullOrEmpty(profile.Name))
            {
                InputUser.Name = profile.Name;
            }

            var role = await mRoleManager.FindByNameAsync(AccountPrincipal.FindFirstValue(ClaimTypes.Role));
            if (role != null)
                InputUser.AccountType = role.Id;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadAsync();

            if (Account == null)
                return NotFound();

            return Page();
        }

        public class UserBanModel
        {
            public int UserId { get; set; }
            public int ReasonId { get; set; }
            public string Description { get; set; }
        }

        public class UserInputModel
        {
            [StringLength(127, MinimumLength = 6)]
            public string Name { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [StringLength(127, MinimumLength = 6)]
            public string UserName { get; set; }

            [Required]
            public bool Password { get; set; }

            [Required]
            public bool TwoFactorEnabled { get; set; }

            [Required]
            public int AccountType { get; set; }

            [DataType(DataType.Date)]
            public DateTime? BirthDate { get; set; }

            public string Gender { get; set; }

            [DataType(DataType.Text)]
            public string Pronoun { get; set; }
            
            [DataType(DataType.Text)]
            public string SocialName { get; set; }

            [DataType(DataType.Text)]
            public string Locality { get; set; }
        }
    }
}
