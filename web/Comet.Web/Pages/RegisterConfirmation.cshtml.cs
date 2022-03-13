using System.Text;
using System.Threading.Tasks;
using Comet.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Comet.Web.Pages
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly IEmailSender mEmailSender;
        private readonly UserManager<ApplicationUser> mUserManager;

        public RegisterConfirmationModel(UserManager<ApplicationUser> userManager, IEmailSender sender)
        {
            mUserManager = userManager;
            mEmailSender = sender;
        }

        public string Email { get; set; }

        public string EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (email == null) return RedirectToPage("/Index");

            ApplicationUser user = await mUserManager.FindByEmailAsync(email);
            if (user == null) return NotFound($"Unable to load user with email '{email}'.");

            Email = email;

            string userId = await mUserManager.GetUserIdAsync(user);
            string code = await mUserManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            EmailConfirmationUrl = Url.Page(
                "/Account/ConfirmEmail",
                null,
                new {area = "Identity", userId, code, returnUrl},
                Request.Scheme);

            return Page();
        }
    }
}