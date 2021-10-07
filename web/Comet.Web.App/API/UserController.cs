using Comet.Web.App.Data;
using Comet.Web.App.Services;
using Comet.Web.App.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Comet.Web.App.API
{
    [Route("api/v1/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext mDbContext;
        private readonly UserManager mUserManager;
        private readonly LanguageService mLanguageService;
        private readonly SignInManager mSignInManager;

        public UserController(ApplicationDbContext dbContext, UserManager userManager, LanguageService language, SignInManager signInManager)
        {
            mDbContext = dbContext;
            mUserManager = userManager;
            mLanguageService = language;
            mSignInManager = signInManager;
        }

        [HttpPost(Name = "ChangePassword")]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordModel model)
        {
            var user = await mUserManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{mUserManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await mUserManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return StatusCode(400, ModelState);
            }

            await mSignInManager.RefreshSignInAsync(user);
            return Ok();
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
            [Display(Name = "NewPassword")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "ConfirmNewPassword")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }
    }
}
