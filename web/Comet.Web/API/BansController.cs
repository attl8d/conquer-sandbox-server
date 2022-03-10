using Comet.Web.Areas.Panel.Models;
using Comet.Web.Data.Entities;
using Comet.Web.Services;
using Comet.Web.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Comet.Web.Services.Security;

namespace Comet.Web.API
{
    [ApiVersion("1")]
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize(Policy = Moderator)]
    public class BansController : ControllerBase
    {

        private readonly UserManager mUserManager;
        private readonly LanguageService mLanguage;
        private readonly SignInManager mSignInManager;
        private readonly RoleManager mRoleManager;
        private readonly BanService mBanService;

        public BansController(UserManager userManager,
            LanguageService language,
            SignInManager signinManager,
            RoleManager roleManager,
            BanService banService)
        {
            mUserManager = userManager;
            mLanguage = language;
            mSignInManager = signinManager;
            mRoleManager = roleManager;
            mBanService = banService;
        }

        [ActionName("")]
        [HttpGet, MapToApiVersion("1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBansV1Async(int page, int ipp = 25)
        {
            return Ok(await mBanService.GetAsync(page, ipp));
        }

        [ActionName("QueryBanReasons")]
        [HttpGet, MapToApiVersion("1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> QueryBanReasonsV1Async(int type)
        {
            List<AdminBanReasonModel> model = new List<AdminBanReasonModel>();
            foreach (var reason in await mBanService.GetBanReasonsAsync(type))
            {
                model.Add(new AdminBanReasonModel
                {
                    Id = reason.Id,
                    Type = reason.Type,
                    GameType = reason.GameType,
                    Name = mLanguage.GetString(reason.Name)
                });
            }
            return Ok(model);
        }

        [ActionName("QueryAccountBans")]
        [HttpGet, MapToApiVersion("1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> QueryAccountBansV1Async(int userId)
        {
            ApplicationUser user = await mUserManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return NotFound();

            var bans = await mUserManager.GetBanInformationAsync(user);
            return Ok(bans.Account);
        }

        [ActionName("GetUserBanFactor")]
        [HttpPost, MapToApiVersion("1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserBanFactorV1Async([FromBody] UserCheckBanModel model)
        {
            ApplicationUser user = await mUserManager.FindByIdAsync(model.UserId.ToString());
            if (user == null)
                return NotFound();

            var factor = await mUserManager.QueryNextBanTermAsync(user, model.Reason);
            if (factor == null)
                return BadRequest();

            return Ok(factor);
        }

        [ActionName("DoBanAccount")]
        [HttpPost, MapToApiVersion("1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DoBanAccountV1Async([FromBody] UserBanModel model)
        {
            if (!User.HasPermission(Claims.BanAccount))
                return Unauthorized();

            ApplicationUser user = await mUserManager.FindByIdAsync(model.userId.ToString());
            if (user == null)
                return NotFound(mLanguage.GetString("UserNotFound"));

            ApplicationUser admin = await mUserManager.GetUserAsync(User);
            if (admin == null)
                return Unauthorized();

            var principal = await mSignInManager.CreateUserPrincipalAsync(user);
            if (!User.IsInRole(SuperAdministrator) && principal.IsInRole(Administrator))
                return Unauthorized();

            if (!User.IsInRole(SuperAdministrator) && principal.HasPermission(Claims.CannotBeBanned))
                return Unauthorized(mLanguage.GetString("UserCannotBeBanned"));

            return StatusCode(await mBanService.BanAccountAsync(admin, user, model.reason, model.description));
        }

        [ActionName("CancelBan")]
        [HttpPatch, MapToApiVersion("1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelBanV1Async()
        {
            return Ok();
        }

        [ActionName("ForgiveBan")]
        [HttpPatch, MapToApiVersion("1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ForgiveBanV1Async()
        {
            return Ok();
        }

        [ActionName("FalsePositiveBan")]
        [HttpPatch, MapToApiVersion("1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FalsePositiveBanV1Async()
        {
            return Ok();
        }

        public class UserRemoveBalModel
        {
            public int userId { get; set; }
            public int banId { get; set; }
        }

        public class UserBanModel
        {
            public int userId { get; set; }
            public int reason { get; set; }
            public string description { get; set; }
        }

        public class UserCheckBanModel
        {
            public int UserId { get; set; }
            public int Reason { get; set; }
        }
    }
}
