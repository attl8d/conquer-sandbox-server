using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
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
    public class ForgotPasswordModel : PageModel
    {
        private readonly IEmailSender mEmailSender;
        private readonly UserManager<ApplicationUser> mUserManager;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            mUserManager = userManager;
            mEmailSender = emailSender;
        }

        [BindProperty] public InputModel Input { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await mUserManager.FindByEmailAsync(Input.Email);
                if (user == null || !await mUserManager.IsEmailConfirmedAsync(user))
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                string code = await mUserManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                string callbackUrl = Url.Page(
                    "/ResetPassword",
                    null,
                    new {code},
                    Request.Scheme);

                await mEmailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }

        public class InputModel
        {
            [Required] [EmailAddress] public string Email { get; set; }
        }
    }
}