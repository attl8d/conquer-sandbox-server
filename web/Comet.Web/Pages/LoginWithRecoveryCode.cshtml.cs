using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Comet.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Comet.Web.Pages
{
    [AllowAnonymous]
    public class LoginWithRecoveryCodeModel : PageModel
    {
        private readonly ILogger<LoginWithRecoveryCodeModel> mLogger;
        private readonly SignInManager<ApplicationUser> mSignInManager;

        public LoginWithRecoveryCodeModel(SignInManager<ApplicationUser> signInManager,
                                          ILogger<LoginWithRecoveryCodeModel> logger)
        {
            mSignInManager = signInManager;
            mLogger = logger;
        }

        [BindProperty] public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            ApplicationUser user = await mSignInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) throw new InvalidOperationException("Unable to load two-factor authentication user.");

            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (!ModelState.IsValid) return Page();

            ApplicationUser user = await mSignInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) throw new InvalidOperationException("Unable to load two-factor authentication user.");

            string recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty);

            SignInResult result = await mSignInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                mLogger.LogInformation("User with ID '{UserId}' logged in with a recovery code.", user.Id);
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }

            if (result.IsLockedOut)
            {
                mLogger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                return RedirectToPage("./Lockout");
            }

            mLogger.LogWarning("Invalid recovery code entered for user with ID '{UserId}' ", user.Id);
            ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
            return Page();
        }

        public class InputModel
        {
            [BindProperty]
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Recovery Code")]
            public string RecoveryCode { get; set; }
        }
    }
}