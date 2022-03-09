using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Comet.Web.Data.Entities;
using Comet.Web.Services;
using Comet.Web.Data;
using static Comet.Web.Pages.LoginModel;
using Comet.Web.Services.Password;
using Comet.Web.Model.Identity;

namespace Comet.Web.Pages
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> mSignInManager;
        private readonly UserManager<ApplicationUser> mUserManager;
        private readonly ILogger<RegisterModel> mLogger;
        private readonly IEmailSender mEmailSender;
        private readonly LanguageService mLanguage;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            LanguageService languageService
            )
        {
            mUserManager = userManager;
            mSignInManager = signInManager;
            mLogger = logger;
            mEmailSender = emailSender;
            mLanguage = languageService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<ExternalLoginSchema> ExternalLogins { get; set; }
        public Dictionary<string, string> AvailableLanguages = new Dictionary<string, string>();
        public Dictionary<int, string> SecurityQuestions = new Dictionary<int, string>();

        public class InputModel
        {
            [Required]
            [StringLength(128, MinimumLength = 4)]
            public string UserName { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [StringLength(32, MinimumLength = 8)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password")]
            public string ConfirmPassword { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [StringLength(16, MinimumLength = 6)]
            public string SecurityCode { get; set; }

            [Required] public string Language { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [StringLength(64, MinimumLength = 4)]
            public string RealName { get; set; }

            [Required][DataType(DataType.Date)] public DateTime Birthday { get; set; }

            [Required]
            [DataType(DataType.PhoneNumber)]
            public string Phone { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            AvailableLanguages = LanguageService.AvailableLanguages.ToDictionary(x => x.LanguageCultureName, y => y.LanguageFullName);

            await SetExternalLoginsAsync();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            AvailableLanguages = LanguageService.AvailableLanguages.ToDictionary(x => x.LanguageCultureName, y => y.LanguageFullName);

            await SetExternalLoginsAsync();
            if (ModelState.IsValid)
            {
                if ((DateTime.Now - Input.Birthday).TotalDays / 365 < 12)
                {
                    ModelState.AddModelError("Underage", mLanguage.GetString("RegisterUnderage"));
                    return Page();
                }

                if (!ulong.TryParse(Input.SecurityCode, out ulong securityCode))
                {
                    ModelState.AddModelError("SecurityCode", mLanguage.GetString("SecurityCodeInteger"));
                    return Page();
                }

                if (!mLanguage.IsLanguageAvailable(Input.Language))
                    Input.Language = mLanguage.CurrentCulture.LanguageCultureName;

                var user = new ApplicationUser
                {
                    UserName = Input.UserName,
                    Email = Input.Email,
                    PhoneNumber = Input.Phone,
                    Salt = WhirlpoolPasswordService<ApplicationUser>.GenerateSalt(),
                    SecurityStamp = Guid.NewGuid().ToString(),
                    CreationDate = DateTime.Now,
                    SecurityCode = securityCode
                };
                var result = await mUserManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    mLogger.LogInformation("User created a new account with password.");

                    await mUserManager.AddToRoleAsync(user, Security.Member);
                    await mUserManager.AddClaimAsync(user, new Claim(CustomClaimTypes.Language, Input.Language));
                    await mUserManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, Input.RealName));
                    await mUserManager.AddClaimAsync(user, new Claim(ClaimTypes.DateOfBirth, Input.Birthday.ToString("s")));

                    var code = await mUserManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code, returnUrl },
                        protocol: Request.Scheme);

                    await mEmailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (mUserManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
                    }
                    else
                    {
                        await mSignInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private async Task SetExternalLoginsAsync()
        {
            ExternalLogins = (await mSignInManager.GetExternalAuthenticationSchemesAsync())
                .Select(x =>
                {
                    switch (x.Name.ToUpper())
                    {
                        case "FACEBOOK":
                            return new ExternalLoginSchema
                            {
                                Icon = "fa fa-facebook",
                                Name = x.Name,
                                DisplayName = x.DisplayName,
                                Class = "btn-primary"
                            };
                        case "GOOGLE":
                            return new ExternalLoginSchema
                            {
                                Icon = "fa fa-google",
                                Name = x.Name,
                                DisplayName = x.DisplayName,
                                Class = "btn-danger"
                            };
                        case "MICROSOFT":
                            return new ExternalLoginSchema
                            {
                                Icon = "fa fa-windows",
                                Name = x.Name,
                                DisplayName = x.DisplayName,
                                Class = "btn-info"
                            };
                        default:
                            return new ExternalLoginSchema
                            {
                                Icon = "fa fa-lock",
                                Name = x.Name,
                                DisplayName = x.DisplayName,
                                Class = "btn-secondary"
                            };
                    }
                })
                .ToList();
        }
    }
}
