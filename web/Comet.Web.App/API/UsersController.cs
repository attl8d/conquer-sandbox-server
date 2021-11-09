using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Services;
using Comet.Web.App.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static Comet.Web.App.Services.Security;

namespace Comet.Web.App.API
{
    [Route("api/v1/[controller]/[action]")]
    [ApiController]
    [Authorize(Policy = Moderator)]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext mDbContext;
        private readonly UserManager mUserManager;
        private readonly LanguageService mLanguage;
        private readonly SignInManager mSigninManager;

        public UsersController(ApplicationDbContext dbContext, UserManager userManager, SignInManager signInManager, LanguageService language)
        {
            mDbContext = dbContext;
            mUserManager = userManager;
            mLanguage = language;
            mSigninManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(int page, int ipp = 10)
        {
            if (!User.HasPermission(Claims.ViewUsers))
                return Forbid();

            var dbResult = await mDbContext.Users
                .Skip(Math.Max(0, (page - 1)) * ipp)
                .Take(ipp)
                .ToListAsync();

            List<object> list = new List<object>();
            foreach (var user in dbResult)
            {
                string role = mLanguage.GetString(Roles.Member);
                var userRole = mDbContext.UserRoles.FirstOrDefault(x => x.UserId == user.Id);
                if (userRole != null)
                {
                    var dbRole = mDbContext.Roles.FirstOrDefault(x => x.Id == userRole.RoleId);
                    if (dbRole != null)
                        role = mLanguage.GetString(dbRole.Name);
                }

                list.Add(new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    role,
                    user.CreationDate
                });
            }
            return Ok(list);
        }

        [HttpGet(Name = "GetClaimsByUser")]
        public async Task<IActionResult> GetClaimsByUserAsync(int userId)
        {
            var claims = await mDbContext.UserClaims.Where(x => x.UserId == userId 
                && x.ClaimType.StartsWith("Permission.")).ToListAsync();

            var result = new List<object>();
            foreach (var claim in claims)
            {
                result.Add(new
                {
                    claim.Id,
                    ClaimType = claim.ClaimType.Split(new char[] { '.' }, 2)[1],
                    claim.ClaimValue,
                    ClaimName = (Enum.TryParse<Claims>(claim.ClaimType, out var eClaim) ? mLanguage.GetString(eClaim.ToPermissionString()) : mLanguage.GetString(claim.ClaimType))
                });
            }
            return Ok(result);
        }

        [Authorize(Policy = Administrator)]
        [HttpPost(Name = "SetClaimToUser")]
        public async Task<IActionResult> SetClaimToUserAsync([FromBody] UserClaimModel model)
        {
            if (!Enum.TryParse(model.claimType, out Claims claimType))
                return BadRequest("Invalid claim type.");

            var userClaim = await mDbContext.UserClaims.FirstOrDefaultAsync(x => x.UserId == model.userId && x.ClaimType.Equals(claimType.ToPermissionString()));
            if (userClaim != null)
                return Ok();

            if (model.claimType.Equals(Claims.Super.ToString()))
                return Forbid();

            userClaim = new ApplicationUserClaim
            {
                ClaimType = claimType.ToPermissionString(),
                ClaimValue = "1",
                UserId = model.userId
            };
            mDbContext.UserClaims.Update(userClaim);
            await mDbContext.SaveChangesAsync();
            return Ok(userClaim);
        }

        [Authorize(Policy = Administrator)]
        [HttpDelete(Name = "DeleteClaimFromUser")]
        public async Task<IActionResult> DeleteClaimFromUserAsync([FromBody] UserClaimModel model)
        {
            if (!Enum.TryParse(model.claimType, out Claims claimType))
                return BadRequest("Invalid claim type.");

            var userClaim = await mDbContext.UserClaims.FirstOrDefaultAsync(x => x.UserId == model.userId && x.ClaimType.Equals(claimType.ToPermissionString()));
            if (userClaim == null)
                return NotFound($"Claim '{model.claimType}' not found for the selected user.");

            if (userClaim.ClaimType.Equals(Claims.Super.ToString()))
                return Forbid();

            mDbContext.UserClaims.Remove(userClaim);
            await mDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet(Name = "QueryAccountBans")]
        public async Task<IActionResult> QueryAccountBansAsync(int userId)
        {
            ApplicationUser user = await mUserManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return NotFound();

            var bans = await mUserManager.GetBanInformationAsync(user);

            return Ok(bans.Account);
        }

        [HttpPost(Name = "GetUserBanFactor")]
        public async Task<IActionResult> GetUserBanFactorAsync([FromBody] UserCheckBanModel model)
        {
            ApplicationUser user = await mUserManager.FindByIdAsync(model.userId.ToString());
            if (user == null)
                return NotFound();

            var factor = await mUserManager.QueryNextBanTermAsync(user, model.reason);
            if (factor == null)
                return BadRequest();

            return Ok(factor);
        }

        [HttpPost(Name = "DoBanAccount")]
        public async Task<IActionResult> DoBanAccountAsync([FromBody] UserBanModel model)
        {
            if (!User.HasPermission(Claims.BanAccount))
                return Unauthorized();

            ApplicationUser user = await mUserManager.FindByIdAsync(model.userId.ToString());
            if (user == null)
                return NotFound(mLanguage.GetString("UserNotFound"));

            var info = await mUserManager.GetBanInformationAsync(user);
            if (info == null)
                return NotFound(mLanguage.GetString("NoUserData"));

            var term = await mUserManager.QueryNextBanTermAsync(user, model.reason);
            if (term == null)
                return BadRequest(mLanguage.GetString("InvalidBanType"));

            if (info.IsAccountBanned)
                return Ok(mLanguage.GetString("UserAlreadyBanned"));

            ApplicationUser admin = await mUserManager.GetUserAsync(User);
            if (admin == null)
                return Unauthorized();

            var principal = await mSigninManager.CreateUserPrincipalAsync(user);

            if (!User.IsInRole(Roles.SuperAdministrator) && principal.IsInRole(Roles.Administrator))
                return Unauthorized();

            if (!User.IsInRole(Roles.SuperAdministrator) && principal.HasPermission(Claims.CannotBeBanned))
                return Unauthorized(mLanguage.GetString("UserCannotBeBanned"));

            ApplicationUserBan ban = new()
            {
                AccountId = user.Id,
                BannedBy = admin.Id,
                BanTime = DateTime.Now,
                ExpireTime = DateTime.Now.AddMinutes(term.Minutes),
                Data = 0,
                Reason = model.description,
                ReasonId = model.reason
            };

            mDbContext.ApplicationUserBans.Add(ban);
            try
            {
                int result = await mDbContext.SaveChangesAsync();
                if (result <= 0)
                {
                    ModelState.AddModelError("DbError", "Não foi possível gravar as informações no banco de dados.");
                    return StatusCode(500, ModelState);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        public class UserBanModel
        {
            public int userId { get; set; }
            public int reason { get; set; }
            public string description { get; set; }
        }

        public class UserCheckBanModel
        {
            public int userId { get; set; }
            public int reason { get; set; }
        }

        public class UserClaimModel
        {
            public int userId { get; set; }
            public string claimType { get; set; }
        }
    }
}
