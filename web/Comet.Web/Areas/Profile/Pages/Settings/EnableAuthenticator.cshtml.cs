using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Comet.Web.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Comet.Web.Areas.Profile.Pages.Settings
{
    public class EnableAuthenticatorModel : PageModel
    {
        private readonly UserManager<ApplicationUser> mUserManager;
        private readonly ILogger<EnableAuthenticatorModel> mLogger;
        private readonly UrlEncoder mUrlEncoder;

        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        public EnableAuthenticatorModel(
            UserManager<ApplicationUser> userManager,
            ILogger<EnableAuthenticatorModel> logger,
            UrlEncoder urlEncoder)
        {
            mUserManager = userManager;
            mLogger = logger;
            mUrlEncoder = urlEncoder;
        }

        public string SharedKey { get; set; }

        public string AuthenticatorUri { get; set; }

        [TempData] public string[] RecoveryCodes { get; set; }

        [TempData] public string StatusMessage { get; set; }

        [BindProperty] public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
                          MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Verification Code")]
            public string Code { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            ApplicationUser user = await mUserManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{mUserManager.GetUserId(User)}'.");

            await LoadSharedKeyAndQrCodeUriAsync(user);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ApplicationUser user = await mUserManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{mUserManager.GetUserId(User)}'.");

            if (!ModelState.IsValid)
            {
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }

            // Strip spaces and hypens
            string verificationCode = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            bool is2FaTokenValid = await mUserManager.VerifyTwoFactorTokenAsync(
                                       user, mUserManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2FaTokenValid)
            {
                ModelState.AddModelError("Input.Code", "Verification code is invalid.");
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }

            await mUserManager.SetTwoFactorEnabledAsync(user, true);
            string userId = await mUserManager.GetUserIdAsync(user);
            mLogger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app.", userId);

            await mUserManager.AddClaimAsync(user, new Claim("amr", "mfa"));

            StatusMessage = "Your authenticator app has been verified.";

            if (await mUserManager.CountRecoveryCodesAsync(user) == 0)
            {
                IEnumerable<string> recoveryCodes = await mUserManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                RecoveryCodes = recoveryCodes.ToArray();
                return RedirectToPage("./ShowRecoveryCodes");
            }

            return RedirectToPage("./TwoFactorAuthentication");
        }

        private async Task LoadSharedKeyAndQrCodeUriAsync(ApplicationUser user)
        {
            // Load the authenticator key & QR code URI to display on the form
            string unformattedKey = await mUserManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await mUserManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await mUserManager.GetAuthenticatorKeyAsync(user);
            }

            SharedKey = FormatKey(unformattedKey);

            string email = await mUserManager.GetEmailAsync(user);
            AuthenticatorUri = GenerateQrCodeUri(email, unformattedKey);
        }

        public string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            var currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }

            if (currentPosition < unformattedKey.Length) result.Append(unformattedKey.Substring(currentPosition));

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                AuthenticatorUriFormat,
                mUrlEncoder.Encode("World Conquer Online"),
                mUrlEncoder.Encode(email),
                unformattedKey);
        }
    }
}