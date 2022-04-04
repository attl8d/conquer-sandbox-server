using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Comet.Web.Areas.Profile.Models;
using Comet.Web.Data.Entities;
using Comet.Web.Services;
using Comet.Web.Services.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Comet.Web.Areas.Profile.Pages.Settings
{
    [Authorize]
    public class SecurityModel : PageModel
    {
        private readonly UserManager mUserManager;
        private readonly LogService<SecurityModel> mLogger;
        private readonly LanguageService mLanguage;
        private readonly IEmailSender mEmailSender;
        private readonly SignInManager mSignInManager;

        public SecurityModel(UserManager userManager,
                             LogService<SecurityModel> logger,
                             LanguageService language,
                             IEmailSender emailService,
                             SignInManager signInManager)
        {
            mUserManager = userManager;
            mLogger = logger;
            mLanguage = language;
            mEmailSender = emailService;
            mSignInManager = signInManager;
        }

        [TempData] public string StatusMessage { get; set; }

        public ProfileModel Profile { get; private set; }

        [BindProperty] public ChangeEmailModel EmailInput { get; set; }
        [BindProperty] public ChangePasswordModel PasswordInput { get; set; }
        [BindProperty] public SetPasswordModel SetPasswordInput { get; set; }

        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationScheme> OtherLogins { get; set; }
        public bool ShowRemoveButton { get; set; }

        public bool HasPassword { get; private set; }

        private async Task LoadAsync()
        {
            ApplicationUser user = await mUserManager.GetUserAsync(User);
            Profile = await mUserManager.GetUserProfileAsync(user, User);
            Profile.LoadedByOwner = true;

            HasPassword = !string.IsNullOrEmpty(user.PasswordHash);

            CurrentLogins = await mUserManager.GetLoginsAsync(user);
            OtherLogins = (await mSignInManager.GetExternalAuthenticationSchemesAsync())
                          .Where(auth => CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
                          .ToList();
            ShowRemoveButton = user.PasswordHash != null || CurrentLogins.Count > 1;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            ApplicationUser user = await mUserManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{mUserManager.GetUserId(User)}'.");

            ModelState.Clear();
            await TryUpdateModelAsync(EmailInput);

            if (!ModelState.IsValid)
            {
                await LoadAsync();
                return Page();
            }

            string email = await mUserManager.GetEmailAsync(user);
            if (EmailInput.NewEmail != email)
            {
                string userId = await mUserManager.GetUserIdAsync(user);
                string code = await mUserManager.GenerateChangeEmailTokenAsync(user, EmailInput.NewEmail);
                string callbackUrl =
                    Url.Page(
                        "/ConfirmEmailChange",
                        null,
                        new { userId, email = EmailInput.NewEmail, code},
                        Request.Scheme);
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
            ApplicationUser user = await mUserManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{mUserManager.GetUserId(User)}'.");

            ModelState.Clear();
            await TryUpdateModelAsync(PasswordInput);

            if (!ModelState.IsValid)
            {
                await LoadAsync();
                return Page();
            }

            IdentityResult changePasswordResult =
                await mUserManager.ChangePasswordAsync(user, PasswordInput.OldPassword, PasswordInput.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                await mEmailSender.SendEmailAsync(user.Email,
                                                  "FTW! Masters - Tentativa de Alteração de Senha",
                                                  $"Houve uma tentativa de alterar a sua senha do usuário '{user.UserName}' vindo do Endereço de IP: {Request.Host}");

                foreach (IdentityError error in changePasswordResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                await LoadAsync();
                return Page();
            }

            await mSignInManager.RefreshSignInAsync(user);
            await mLogger.LogAsync("User changed their password successfully.",
                                   LogHistory.LogType.ChangeAccountPassword);
            StatusMessage = mLanguage.GetString("PasswordChangeSuccess");

            await mEmailSender.SendEmailAsync(user.Email,
                                              "FTW! Masters - Alteração de Senha",
                                              $"Sua senha acaba de ser alterada para o usuário '{user.UserName}'.");

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            ApplicationUser user = await mUserManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{mUserManager.GetUserId(User)}'.");

            string userId = await mUserManager.GetUserIdAsync(user);
            string email = await mUserManager.GetEmailAsync(user);
            string code = await mUserManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            string callbackUrl = Url.Page(
                "/ConfirmEmail",
                null,
                new { userId, code},
                Request.Scheme);
            await mEmailSender.SendEmailAsync(
                email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSetPasswordAsync()
        {
            ApplicationUser user = await mUserManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{mUserManager.GetUserId(User)}'.");

            ModelState.Clear();
            await TryUpdateModelAsync(SetPasswordInput);

            if (!ModelState.IsValid)
            {
                await LoadAsync();
                return Page();
            }

            IdentityResult addPasswordResult = await mUserManager.AddPasswordAsync(user, SetPasswordInput.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                foreach (IdentityError error in addPasswordResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                await LoadAsync();
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
            ApplicationUser user = await mUserManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID.");

            IdentityResult result = await mUserManager.RemoveLoginAsync(user, loginProvider, providerKey);
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
            string redirectUrl = Url.Page("./Security", "LinkLoginCallback");
            AuthenticationProperties properties =
                mSignInManager.ConfigureExternalAuthenticationProperties(
                    provider, redirectUrl, mUserManager.GetUserId(User));
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetLinkLoginCallbackAsync()
        {
            ApplicationUser user = await mUserManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{user.Id}'.");

            ExternalLoginInfo info = await mSignInManager.GetExternalLoginInfoAsync(user.Id.ToString());
            if (info == null)
                throw new InvalidOperationException(
                    $"Unexpected error occurred loading external login info for user with ID '{user.Id}'.");

            IdentityResult result = await mUserManager.AddLoginAsync(user, info);
            if (!result.Succeeded)
            {
                StatusMessage =
                    "The external login was not added. External logins can only be associated with one account.";
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
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
                          MinimumLength = 6)]
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
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
                          MinimumLength = 6)]
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