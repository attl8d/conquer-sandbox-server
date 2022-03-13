using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Comet.Web.Areas.Panel.Models;
using Comet.Web.Data.Entities;
using Comet.Web.Services;
using Comet.Web.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static Comet.Web.Services.Security;

namespace Comet.Web.API
{
    [ApiVersion("1")]
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize(Policy = Moderator)]
    public class AccountsController : ControllerBase
    {
        private readonly LanguageService mLanguage;
        private readonly RoleManager mRoleManager;
        private readonly SignInManager mSignInManager;
        private readonly UserManager mUserManager;

        public AccountsController(UserManager userManager,
                                  LanguageService language,
                                  SignInManager signinManager,
                                  RoleManager roleManager)
        {
            mUserManager = userManager;
            mLanguage = language;
            mSignInManager = signinManager;
            mRoleManager = roleManager;
        }

        /// <summary>
        ///     Returns a list of users.
        /// </summary>
        /// <remarks>
        ///     This will return a paginated query of all registered users at the moment. This does not return
        ///     sensitive data.
        /// </remarks>
        /// <param name="page">The index of the page.</param>
        /// <param name="ipp">Indexes per page.</param>
        /// <response code="200">A list with the registered users.</response>
        /// <response code="500">Internal server error.</response>
        [ActionName("")]
        [HttpGet(Name = "Get")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(List<AdminUserListModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetV1Async(int page, int ipp = 10)
        {
            if (!User.HasPermission(Claims.ViewUsers))
                return Unauthorized();

            List<ApplicationUser> users = await mUserManager.GetUsersAsync(page, ipp);

            var result = new List<AdminUserListModel>();
            foreach (ApplicationUser user in users)
            {
                string role = mLanguage.GetString(Member);
                string userRole = (await mUserManager.GetRolesAsync(user)).FirstOrDefault();
                if (userRole != null)
                {
                    ApplicationRole dbRole = mRoleManager.Roles.FirstOrDefault(x => x.Name == userRole);
                    if (dbRole != null)
                        role = mLanguage.GetString(dbRole.Name);
                }

                result.Add(new AdminUserListModel
                {
                    Id = user.Id,
                    Role = role,
                    UserName = user.UserName,
                    Email = user.Email,
                    CreationDate = user.CreationDate
                });
            }

            return Ok(result);
        }

        /// <summary>
        ///     The list of claims.
        /// </summary>
        /// <remarks>
        ///     This will return the list of claims of the specified user.
        /// </remarks>
        /// <param name="userId">The identity of the user.</param>
        /// <response code="200">The list of claims of the selected user.</response>
        /// <response code="401">The specified user doesn't exist.</response>
        /// <response code="500">Internal server error.</response>
        [ActionName("GetClaimsByUser")]
        [HttpGet]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(List<AdminUserClaimModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetClaimsByUserV1Async(int userId)
        {
            ApplicationUser user = await mUserManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return NoContent();

            var result = new List<AdminUserClaimModel>();
            IList<Claim> claims = await mUserManager.GetClaimsAsync(user);
            foreach (Claim claim in claims.Where(x => x.Type.StartsWith("Permission.")))
                result.Add(new AdminUserClaimModel
                {
                    ClaimType = claim.Type.Split(new[] {'.'}, 2)[1],
                    ClaimValue = claim.Value,
                    ClaimName = Enum.TryParse(claim.Type, out Claims eClaim)
                                    ? mLanguage.GetString(eClaim.ToPermissionString())
                                    : mLanguage.GetString(claim.Type)
                });
            return Ok(result);
        }

        /// <summary>
        ///     Set a new claim for the specified user.
        /// </summary>
        /// <remarks>
        ///     This will set a claim to the specified user if the logged in user has permission to it.
        ///     Requires Administrator role.
        /// </remarks>
        /// <response code="200">If successfull. Also brings the filled claim.</response>
        /// <response code="204">THe spscified user does not exist.</response>
        /// <response code="400">The specified claim does not exist.</response>
        /// <response code="403">If the user has no permission or tries to append the Super User claim.</response>
        /// <response code="500">Internal server error.</response>
        [Authorize(Policy = Administrator)]
        [ActionName("SetClaimToUser")]
        [HttpPut]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(Claim), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetClaimToUserV1Async([FromBody] UserClaimModel model)
        {
            ApplicationUser user = await mUserManager.FindByIdAsync(model.UserId.ToString());
            if (user == null)
                return NoContent();

            if (model.ClaimType.Equals(Claims.Super.ToString()))
                return StatusCode(403);

            if (!Enum.TryParse(model.ClaimType, out Claims claimType))
                return BadRequest("Invalid claim type.");

            IList<Claim> claims = await mUserManager.GetClaimsAsync(user);
            Claim claim = claims.FirstOrDefault(x => x.Type.Equals(model.ClaimType));

            if (claim != null)
                return Ok(claim);

            claim = new Claim(claimType.ToPermissionString(), "1");
            await mUserManager.AddClaimAsync(user, claim);
            return Ok(claim);
        }

        /// <summary>
        ///     Removes a claim
        /// </summary>
        /// <remarks>
        ///     Removes a claim of the specified user. Requires Administrator role.
        /// </remarks>
        /// <response code="200">Empty response if success or if the user doesn't have the claim.</response>
        /// <response code="400">The user or claim are not valid.</response>
        /// <response code="403">The user doesn't have permission or it's trying to remove the Super User claim.</response>
        /// <response code="500">Internal server error.</response>
        [Authorize(Policy = Administrator)]
        [ActionName("DeleteClaimFromUser")]
        [HttpDelete]
        [MapToApiVersion("1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteClaimFromUserAsync([FromBody] UserClaimModel model)
        {
            ApplicationUser user = await mUserManager.FindByIdAsync(model.UserId.ToString());
            if (user == null)
                return BadRequest("User not valid.");

            if (model.ClaimType.Equals(Claims.Super.ToString()))
                return StatusCode(403);

            if (!Enum.TryParse(model.ClaimType, out Claims claimType))
                return BadRequest("Invalid claim type.");

            IList<Claim> claims = await mUserManager.GetClaimsAsync(user);
            Claim claim = claims.FirstOrDefault(x => x.Type.Equals(claimType.ToPermissionString()));

            if (claim == null)
                return Ok();

            IdentityResult result = await mUserManager.RemoveClaimAsync(user, claim);
            return Ok();
        }

        public class UserClaimModel
        {
            public int UserId { get; set; }
            public string ClaimType { get; set; }
        }
    }
}