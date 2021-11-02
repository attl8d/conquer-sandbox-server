using Comet.Web.App.Areas.Profile.Models;
using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Services;
using Comet.Web.App.Services.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Comet.Web.App.Areas.Profile.Pages.Settings
{
    public class SecurityModel : PageModel
    {
        private readonly UserManager mUserManager;
        private readonly ILogger<SecurityModel> mLogger;
        private readonly LanguageService mLanguage;
        private readonly ApplicationDbContext mDbContext;
        private readonly IEmailSender mEmailSender;
        private readonly SignInManager mSignInManager;

        public SecurityModel(UserManager userManager,
            ILogger<SecurityModel> logger,
            LanguageService language,
            ApplicationDbContext dbContext,
            IEmailSender emailService,
            SignInManager signInManager)
        {
            mUserManager = userManager;
            mLogger = logger;
            mLanguage = language;
            mDbContext = dbContext;
            mEmailSender = emailService;
            mSignInManager = signInManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public ProfileModel Profile { get; private set; }

        [BindProperty] public ChangeEmailModel EmailInput { get; set; }
        [BindProperty] public ChangePasswordModel PasswordInput { get; set; }
        [BindProperty] public SetPasswordModel SetPasswordInput { get; set; }

        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationScheme> OtherLogins { get; set; }
        public bool ShowRemoveButton { get; set; }

        public bool HasPassword { get; private set; }

        private async Task LoadAsync(ApplicationUser user)
        {
            Profile = await mUserManager.GetUserProfileAsync(user);
            Profile.LoadedByOwner = true;
            Profile.CurrentPage = "SETTINGS";

            HasPassword = !string.IsNullOrEmpty(user.PasswordHash);

            CurrentLogins = await mUserManager.GetLoginsAsync(user);
            OtherLogins = (await mSignInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
                .ToList();
            ShowRemoveButton = user.PasswordHash != null || CurrentLogins.Count > 1;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            ApplicationUser user = await mUserManager.GetUserAsync(User);

            if (user == null)
                return LocalRedirect("/");

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await mUserManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{mUserManager.GetUserId(User)}'.");
            }

            ModelState.Clear();
            await TryUpdateModelAsync(EmailInput);

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var email = await mUserManager.GetEmailAsync(user);
            if (EmailInput.NewEmail != email)
            {
                var userId = await mUserManager.GetUserIdAsync(user);
                var code = await mUserManager.GenerateChangeEmailTokenAsync(user, EmailInput.NewEmail);
                var callbackUrl = 
                    Url.Page(
                    "/Account/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, email = EmailInput.NewEmail, code = code },
                    protocol: Request.Scheme);
                await mEmailSender.SendEmailAsync(
                    EmailInput.NewEmail,
                    "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                StatusMessage = mLanguage.GetString("NewVerificationEmailSent");
                return RedirectToPage();
            }

            StatusMessage = mLanguage.GetString("VerificationEmailFail");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            var user = await mUserManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{mUserManager.GetUserId(User)}'.");
            }

            ModelState.Clear();
            await TryUpdateModelAsync(PasswordInput);

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var changePasswordResult = await mUserManager.ChangePasswordAsync(user, PasswordInput.OldPassword, PasswordInput.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                await mEmailSender.SendEmailAsync(user.Email,
                    "FTW! Masters - Tentativa de Alteração de Senha",
                    $"Houve uma tentativa de alterar a sua senha do usuário '{user.UserName}' vindo do Endereço de IP: {Request.Host}");

                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                await LoadAsync(user);
                return Page();
            }

            await mSignInManager.RefreshSignInAsync(user);
            mLogger.LogInformation("User changed their password successfully.");
            StatusMessage = mLanguage.GetString("PasswordChangeSuccess");

            await mEmailSender.SendEmailAsync(user.Email,
                "FTW! Masters - Alteração de Senha",
                $"Sua senha acaba de ser alterada para o usuário '{user.UserName}'.");

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            var user = await mUserManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{mUserManager.GetUserId(User)}'.");
            }

            ModelState.Clear();
            await TryUpdateModelAsync(EmailInput);

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var userId = await mUserManager.GetUserIdAsync(user);
            var email = await mUserManager.GetEmailAsync(user);
            var code = await mUserManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);
            await mEmailSender.SendEmailAsync(
                email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSetPasswordAsync()
        {
            var user = await mUserManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{mUserManager.GetUserId(User)}'.");
            }

            ModelState.Clear();
            await TryUpdateModelAsync(SetPasswordInput);

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var addPasswordResult = await mUserManager.AddPasswordAsync(user, SetPasswordInput.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                foreach (var error in addPasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                await LoadAsync(user);
                return Page();
            }

            await mSignInManager.RefreshSignInAsync(user);
            StatusMessage = mLanguage.GetString("SetPasswordSuccess");

            await mEmailSender.SendEmailAsync(
                user.Email,
                "Definição de Senha",
                $"Você acabou de definir uma nova senha para o seu usuário '{user.UserName}'."
                );

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveLoginAsync(string loginProvider, string providerKey)
        {
            var user = await mUserManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{user.Id}'.");
            }

            var result = await mUserManager.RemoveLoginAsync(user, loginProvider, providerKey);
            if (!result.Succeeded)
            {
                StatusMessage = "The external login was not removed.";
                return RedirectToPage();
            }

            await mSignInManager.RefreshSignInAsync(user);
            StatusMessage = "The external login was removed.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostLinkLoginAsync(string provider)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Page("./Security", pageHandler: "LinkLoginCallback");
            var properties = mSignInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, mUserManager.GetUserId(User));
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetLinkLoginCallbackAsync()
        {
            var user = await mUserManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{user.Id}'.");
            }

            var info = await mSignInManager.GetExternalLoginInfoAsync(user.Id.ToString());
            if (info == null)
            {
                throw new InvalidOperationException($"Unexpected error occurred loading external login info for user with ID '{user.Id}'.");
            }

            var result = await mUserManager.AddLoginAsync(user, info);
            if (!result.Succeeded)
            {
                StatusMessage = "The external login was not added. External logins can only be associated with one account.";
                return RedirectToPage();
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            StatusMessage = "The external login was added.";
            return RedirectToPage();
        }

        public class ChangePasswordModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "CurrentPassword")]
            public string OldPassword { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public class ChangeEmailModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; }
        }

        public class SetPasswordModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }
    }
}