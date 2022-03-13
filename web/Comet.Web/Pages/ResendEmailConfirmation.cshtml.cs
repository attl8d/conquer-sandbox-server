using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Comet.Web.Data.Entities;
using Comet.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Comet.Web.Pages
{
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly IEmailSender mEmailSender;
        private readonly LanguageService mLanguage;
        private readonly UserManager<ApplicationUser> mUserManager;

        public ResendEmailConfirmationModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender,
                                            LanguageService language)
        {
            mUserManager = userManager;
            mEmailSender = emailSender;
            mLanguage = language;
        }

        [BindProperty] public InputModel Input { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            ApplicationUser user = await mUserManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, mLanguage.GetString("VerificationEmailSent"));
                return Page();
            }

            if (user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, mLanguage.GetString("VerificationEmailSent"));
                return Page();
            }

            string userId = await mUserManager.GetUserIdAsync(user);
            string code = await mUserManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            string callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                null,
                new {userId, code},
                Request.Scheme);
            await mEmailSender.SendEmailAsync(
                Input.Email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            ModelState.AddModelError(string.Empty, mLanguage.GetString("VerificationEmailSent"));
            return Page();
        }

        public class InputModel
        {
            [Required] [EmailAddress] public string Email { get; set; }
        }
    }
}