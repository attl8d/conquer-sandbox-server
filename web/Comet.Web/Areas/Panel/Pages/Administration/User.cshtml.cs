using Comet.Web.Areas.Profile.Models;
using Comet.Web.Data.Entities;
using Comet.Web.Model.Identity;
using Comet.Web.Services;
using Comet.Web.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
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
                InputUser.SocialName = profile.SocialName;
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

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadAsync();

            if (!User.HasPermission(Security.Claims.EditUsers))
            {
                ModelState.Clear();
                ModelState.AddModelError("NoPermission", mLanguage.GetString("NoPermission"));
                await LoadAsync();
                return Page();
            }

            if (Account == null)
                return NotFound();

            ModelState.Clear();
            await TryUpdateModelAsync(InputUser);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var currentUser = await mUserManager.GetUserAsync(User);
            if (currentUser.Id == Account.Id)
            {
                ModelState.AddModelError("NoPermission", mLanguage.GetString("CannotUpdateSelf"));
                return Page();
            }

            bool changeUsername = false;
            if (!InputUser.UserName.Equals(Account.UserName))
            {
                var tempUser = await mUserManager.FindByNameAsync(InputUser.UserName);
                if (tempUser != null)
                {
                    ModelState.AddModelError("UserNameInvalid", mLanguage.GetString("UserNameInUse"));
                    return Page();
                }

                changeUsername = true;
            }

            bool changeEmail = false;
            if (!InputUser.Email.Equals(Account.Email))
            {
                var tempUser = await mUserManager.FindByEmailAsync(InputUser.Email);
                if (tempUser != null)
                {
                    ModelState.AddModelError("EmailInvalid", mLanguage.GetString("EmailInUse"));
                    return Page();
                }

                changeEmail = true;
            }

            if (changeUsername)
                await mUserManager.SetUserNameAsync(Account, InputUser.UserName);

            if (changeEmail)
            {
                var userId = await mUserManager.GetUserIdAsync(Account);
                var code = await mUserManager.GenerateChangeEmailTokenAsync(Account, InputUser.Email);
                var callbackUrl =
                    Url.Page(
                    "/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { userId = userId, email = InputUser.Email, code = code },
                    protocol: Request.Scheme);
                await mEmailSender.SendEmailAsync(
                    InputUser.Email,
                    "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
            }

            if (InputUser.Password)
            {
                // reset password ...
                var code = await mUserManager.GeneratePasswordResetTokenAsync(Account);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/ResetPassword",
                    pageHandler: null,
                    values: new { code },
                    protocol: Request.Scheme);

                await mEmailSender.SendEmailAsync(
                    Account.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
            }

            if ((InputUser.Password && Account.TwoFactorEnabled) || (!InputUser.TwoFactorEnabled && Account.TwoFactorEnabled))
            {
                if (Account.Id != 1 && !(AccountPrincipal.IsInRole(SuperAdministrator) && !User.IsInRole(SuperAdministrator)))
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
            claimsPairs.Add(ClaimTypes.DateOfBirth, InputUser.BirthDate?.ToString("s"));
            claimsPairs.Add(ClaimTypes.Gender, InputUser.Gender);
            claimsPairs.Add(CustomClaimTypes.Pronoum, InputUser.Pronoun);
            claimsPairs.Add(CustomClaimTypes.SocialName, InputUser.SocialName);
            claimsPairs.Add(ClaimTypes.Locality, InputUser.Locality);
            claimsPairs.Add(ClaimTypes.Name, InputUser.Name);

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
            public string SocialName { get; set; }

            [DataType(DataType.Text)]
            public string Locality { get; set; }
        }
    }
}
