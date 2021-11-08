using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static Comet.Web.App.Services.Security;

namespace Comet.Web.App.Areas.Panel.Pages.Admin
{
    [Authorize(Policy = Moderator)]
    public class UserModel : PageModel
    {
        private readonly ApplicationDbContext mDbContext;
        private readonly UserManager mUserManager;
        private readonly SignInManager mSigninManager;

        public UserModel(ApplicationDbContext dbContext,
            UserManager userManager,
            SignInManager signInManager)
        {
            mDbContext = dbContext;
            mUserManager = userManager;
            mSigninManager = signInManager;
        }

        public Dictionary<string, Claims> Claims = new Dictionary<string, Claims>();

        public List<ApplicationUserBanReason> BanReasons { get; private set; }

        [BindProperty(SupportsGet = true)] public int UserId { get; set; }

        [BindProperty] public UserInputModel InputUser { get; set; }
        [BindProperty] public UserBanModel InputBan { get; set; }

        public ApplicationUser Account { get; set; }
        public ClaimsPrincipal AccountPrincipal { get; set; }

        private async Task LoadAsync()
        {
            Account = await mDbContext.Users.FindAsync(UserId);
            if (Account == null)
                return;

            Claims = new Dictionary<string, Claims>(Enum.GetValues<Claims>().ToDictionary(x => x.ToString()));

            AccountPrincipal = await mSigninManager.CreateUserPrincipalAsync(Account);

            BanReasons = await mDbContext.ApplicationUserBanReasons.Where(x => x.Type == 0).ToListAsync();

            var profile = await mUserManager.GetUserProfileAsync(Account);
            InputUser ??= new UserInputModel()
            {
                Email = Account.Email,
                UserName = Account.UserName,
                TwoFactorEnabled = Account.TwoFactorEnabled
                //BirthDate = Account.Birthdate
                //Gender = profile.PersonalInfo[Profile.Models.PersonalInfoType.Gender],
                //Locality = profile.PersonalInfo[Profile.Models.PersonalInfoType.Locality],
                //Pronoun = profile.PersonalInfo[Profile.Models.PersonalInfoType.Pronoun]
            };

            if (profile.PersonalInfo.ContainsKey(Profile.Models.PersonalInfoType.DateOfBirth))
            {
                InputUser.BirthDate = DateTime.Parse(profile.PersonalInfo[Profile.Models.PersonalInfoType.DateOfBirth]);
            }

            if (profile.PersonalInfo.ContainsKey(Profile.Models.PersonalInfoType.Gender))
            {
                InputUser.Gender = profile.PersonalInfo[Profile.Models.PersonalInfoType.Gender];
            }

            if (profile.PersonalInfo.ContainsKey(Profile.Models.PersonalInfoType.Locality))
            {
                InputUser.Locality = profile.PersonalInfo[Profile.Models.PersonalInfoType.Locality];
            }

            if (profile.PersonalInfo.ContainsKey(Profile.Models.PersonalInfoType.Pronoun))
            {
                InputUser.Pronoun = profile.PersonalInfo[Profile.Models.PersonalInfoType.Pronoun];
            }

            var role = await mDbContext.Roles.FirstOrDefaultAsync(x => x.Name.Equals(AccountPrincipal.FindFirstValue(ClaimTypes.Role)));
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
            [Required]
            [StringLength(127, MinimumLength = 6)]
            public string Name { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [StringLength(127, MinimumLength = 6)]
            public string UserName { get; set; }

            [StringLength(128, MinimumLength = 8)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

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
            public string Locality { get; set; }
        }
    }
}
