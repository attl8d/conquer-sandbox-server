using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Services;
using Comet.Web.App.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        public class UserClaimModel
        {
            public int userId { get; set; }
            public string claimType { get; set; }
        }
    }
}
