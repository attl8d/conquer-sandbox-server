using Comet.Web.App.Areas.Profile.Models;
using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Model.Account;
using Comet.Web.App.Services;
using Comet.Web.App.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
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
        private readonly LanguageService mLanguage;
        private readonly IEmailSender mEmailSender;

        public UserModel(ApplicationDbContext dbContext,
            UserManager userManager,
            SignInManager signInManager,
            LanguageService languageService,
            IEmailSender emailService)
        {
            mDbContext = dbContext;
            mUserManager = userManager;
            mSigninManager = signInManager;
            mLanguage = languageService;
            mEmailSender = emailService;
        }

        public Dictionary<string, Claims> Claims = new Dictionary<string, Claims>();

        public List<ApplicationUserBanReason> BanReasons { get; private set; }

        [BindProperty(SupportsGet = true)] public int UserId { get; set; }

        [BindProperty] public UserInputModel InputUser { get; set; }
        [BindProperty] public UserBanModel InputBan { get; set; }

        public ApplicationUser Account { get; set; }
        public ClaimsPrincipal AccountPrincipal { get; set; }
        public BanReport BanReport { get; set; }

        private async Task LoadAsync()
        {
            Account = await mDbContext.Users.FindAsync(UserId);
            if (Account == null)
                return;

            Claims = new Dictionary<string, Claims>(Enum.GetValues<Claims>().Where(x => x != Security.Claims.Super).ToDictionary(x => x.ToString()));

            AccountPrincipal = await mSigninManager.CreateUserPrincipalAsync(Account);

            BanReasons = await mDbContext.ApplicationUserBanReasons.Where(x => x.Type == 0).ToListAsync();

            BanReport = await mUserManager.GetBanInformationAsync(Account);

            var profile = await mUserManager.GetUserProfileAsync(Account);
            InputUser ??= new UserInputModel()
            {
                Email = Account.Email,
                UserName = Account.UserName,
                TwoFactorEnabled = Account.TwoFactorEnabled
            };

            if (profile.PersonalInfo.ContainsKey(PersonalInfoType.DateOfBirth))
            {
                InputUser.BirthDate = DateTime.Parse(profile.PersonalInfo[PersonalInfoType.DateOfBirth]);
            }

            if (profile.PersonalInfo.ContainsKey(PersonalInfoType.Gender))
            {
                InputUser.Gender = profile.PersonalInfo[PersonalInfoType.Gender];
            }

            if (profile.PersonalInfo.ContainsKey(PersonalInfoType.Locality))
            {
                InputUser.Locality = profile.PersonalInfo[PersonalInfoType.Locality];
            }

            if (profile.PersonalInfo.ContainsKey(PersonalInfoType.Pronoun))
            {
                InputUser.Pronoun = profile.PersonalInfo[PersonalInfoType.Pronoun];
            }

            if (profile.PersonalInfo.ContainsKey(PersonalInfoType.Name))
            {
                InputUser.Name = profile.PersonalInfo[PersonalInfoType.Name];
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!User.HasPermission(Security.Claims.EditUsers))
            {
                ModelState.Clear();
                ModelState.AddModelError("NoPermission", mLanguage.GetString("NoPermission"));
                await LoadAsync();
                return Page();
            }

            await LoadAsync();

            if (Account == null)
                return NotFound();

            ModelState.Clear();
            await TryUpdateModelAsync(InputUser);

            if (!ModelState.IsValid)
            {
                await LoadAsync();
                return Page();
            }

            if (!InputUser.UserName.Equals(Account.UserName))
            {
                if (!mDbContext.Users.Any(x => x.UserName.Equals(InputUser.UserName)))
                {
                    await mUserManager.SetUserNameAsync(Account, InputUser.UserName);
                }
                else
                {
                    ModelState.AddModelError("UserNameInvalid", mLanguage.GetString("UserNameInUse"));
                }
            }

            if (!InputUser.Email.Equals(Account.Email))
            {
                if (!mDbContext.Users.Any(x => x.Email.Equals(InputUser.Email)))
                {
                    var userId = await mUserManager.GetUserIdAsync(Account);
                    var code = await mUserManager.GenerateChangeEmailTokenAsync(Account, InputUser.Email);
                    var callbackUrl =
                        Url.Page(
                        "/Account/ConfirmEmailChange",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, email = InputUser.Email, code = code },
                        protocol: Request.Scheme);
                    await mEmailSender.SendEmailAsync(
                        InputUser.Email,
                        "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                }
                else
                {
                    ModelState.AddModelError("EmailInvalid", mLanguage.GetString("EmailInUse"));
                }
            }

            if (InputUser.Password)
            {
                // reset password ...
                var code = await mUserManager.GeneratePasswordResetTokenAsync(Account);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                await mEmailSender.SendEmailAsync(
                    Account.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
            }

            if ((InputUser.Password && Account.TwoFactorEnabled) || (!InputUser.TwoFactorEnabled && Account.TwoFactorEnabled))
            {
                if (Account.Id != 1 && !(AccountPrincipal.IsInRole(Roles.SuperAdministrator) && !User.IsInRole(Roles.SuperAdministrator)))
                {
                    // remove two factor
                    var disable2faResult = await mUserManager.SetTwoFactorEnabledAsync(Account, false);
                    if (!disable2faResult.Succeeded)
                    {
                        ModelState.AddModelError("TwoFactorError", mLanguage.GetString("CouldNotDisable2fa"));
                    }
                }                
            }

            Dictionary<string, string> claimsPairs = new();
            claimsPairs.Add(PersonalInfoType.DateOfBirth.ToClaimString(), InputUser.BirthDate?.ToString("s"));
            claimsPairs.Add(PersonalInfoType.Gender.ToClaimString(), InputUser.Gender);
            claimsPairs.Add(PersonalInfoType.Pronoun.ToClaimString(), InputUser.Pronoun);
            claimsPairs.Add(PersonalInfoType.Locality.ToClaimString(), InputUser.Locality);
            claimsPairs.Add(PersonalInfoType.Name.ToClaimString(), InputUser.Name);

            foreach (var claim in claimsPairs)
            {
                Claim temp = User.FindFirst(claim.Key);

                if (string.IsNullOrEmpty(claim.Value))
                {
                    if (temp != null)
                        await mUserManager.RemoveClaimAsync(Account, temp);
                    continue;
                }

                Claim now = new(claim.Key, claim.Value);

                if (!string.IsNullOrEmpty(claim.Value))
                {
                    if (temp != null)
                    {
                        await mUserManager.ReplaceClaimAsync(Account, temp, now);
                    }
                    else
                    {
                        await mUserManager.AddClaimAsync(Account, now);
                    }
                }
            }

            await mSigninManager.RefreshSignInAsync(Account);

            return RedirectToPage();
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
            public string Locality { get; set; }
        }
    }
}
