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
    [Authorize(Policy = Administrator)]
    public class RolesController : ControllerBase
    {
        private readonly LanguageService mLanguage;
        private readonly RoleManager mRoleManager;
        private readonly SignInManager mSigninManager;
        private readonly UserManager mUserManager;

        public RolesController(UserManager userManager,
                               LanguageService language,
                               SignInManager signinManager,
                               RoleManager roleManager)
        {
            mUserManager = userManager;
            mLanguage = language;
            mSigninManager = signinManager;
            mRoleManager = roleManager;
        }

        /// <summary>
        ///     Returns a list of roles.
        /// </summary>
        /// <remarks>
        ///     This will return a paginated query of all registered roles at the moment.
        /// </remarks>
        /// <param name="page">The index of the page.</param>
        /// <param name="ipp">Indexes per page.</param>
        /// <response code="200">A list with the registered roles.</response>
        /// <response code="401">User has no permission.</response>
        /// <response code="500">Internal server error.</response>
        [ActionName("")]
        [HttpGet]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(List<AdminRoleListModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync(int page, int ipp = 10)
        {
            if (!User.HasPermission(Claims.ViewRoles))
                return Unauthorized();

            List<ApplicationRole> roles = await mRoleManager.GetRolesAsync(page, ipp);
            var result = new List<AdminRoleListModel>();
            foreach (ApplicationRole role in roles)
                result.Add(new AdminRoleListModel
                {
                    Id = role.Id,
                    Name = role.Name,
                    CreationDate = role.CreationDate,
                    ModifiedDate = role.ModifiedDate,
                    NormalizedName = role.NormalizedName,
                    Count = (await mUserManager.GetUsersInRoleAsync(role.Name)).Count
                });
            return Ok(result);
        }

        /// <summary>
        ///     Returns a list of users.
        /// </summary>
        /// <remarks>
        ///     This will return a paginated query of all registered users in specific role at the moment.
        /// </remarks>
        /// <param name="roleId">The identity of the role.</param>
        /// <param name="page">The index of the page.</param>
        /// <param name="ipp">Indexes per page.</param>
        /// <response code="200">A list with the registered roles.</response>
        /// <response code="401">User has no permission.</response>
        /// <response code="500">Internal server error.</response>
        [ActionName("GetUsersByRole")]
        [HttpGet]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(List<AdminUserListModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsersByRoleV1Async(int roleId, int page = 0, int ipp = 10)
        {
            if (!User.HasPermission(Claims.ViewRoles))
                return Forbid();

            List<ApplicationUser> users = await mUserManager.GetUsersByRoleAsync(roleId, page, ipp);
            List<AdminUserListModel> result = users.Select(x => new AdminUserListModel
            {
                Id = x.Id,
                Email = x.Email,
                UserName = x.UserName,
                CreationDate = x.CreationDate
            }).ToList();
            return Ok(result);
        }

        /// <summary>
        ///     Returns a list of claims.
        /// </summary>
        /// <remarks>
        ///     This will return a list of all registered claims for a specific role at the moment.
        /// </remarks>
        /// <param name="roleId">The identity of the role.</param>
        /// <response code="200">A list with the registered roles.</response>
        /// <response code="204">Invalid role ID.</response>
        /// <response code="401">User has no permission.</response>
        /// <response code="500">Internal server error.</response>
        [ActionName("GetClaimsByRole")]
        [HttpGet]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(List<AdminRoleClaimModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetClaimsByRoleV1Async(int roleId)
        {
            if (!User.HasPermission(Claims.ViewClaims))
                return Unauthorized();

            ApplicationRole role = await mRoleManager.FindByIdAsync(roleId.ToString());
            if (role == null)
                return NoContent();

            IList<Claim> claims = await mRoleManager.GetClaimsAsync(role);
            var result = new List<AdminRoleClaimModel>();
            foreach (Claim claim in claims)
                result.Add(new AdminRoleClaimModel
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
        ///     Insert a new claim.
        /// </summary>
        /// <remarks>
        ///     This will insert a new claim for the required role. User claims override role claims.
        /// </remarks>
        /// <response code="200">The claim has been added.</response>
        /// <response code="401">User has no permission.</response>
        /// <response code="500">Internal server error.</response>
        [ActionName("SetClaimToRole")]
        [HttpPut]
        [MapToApiVersion("1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetClaimToRoleV1Async([FromBody] RoleClaimModel model)
        {
            if (!User.HasPermission(Claims.EditRoleClaims))
                return Unauthorized();

            if (model.claimType.Equals(Claims.Super.ToString()))
                return Unauthorized();

            if (!Enum.TryParse(model.claimType, out Claims claimType))
                return BadRequest("Invalid claim type.");

            ApplicationRole role = await mRoleManager.FindByIdAsync(model.roleId.ToString());
            if (role == null)
                return BadRequest("Unexistent Role.");

            ApplicationRoleClaim roleClaim =
                await mRoleManager.GetRoleClaimAsync(model.roleId, claimType.ToPermissionString());
            if (roleClaim != null)
                return Ok("Role already owns claim.");

            Claim claim = new(claimType.ToPermissionString(), "1");
            if ((await mRoleManager.AddClaimAsync(role, claim)).Succeeded)
                return Ok(claim);

            return StatusCode(500);
        }

        /// <summary>
        ///     Delete a claim.
        /// </summary>
        /// <remarks>
        ///     This will remove a claim from a role. User claims override role claims.
        /// </remarks>
        /// <response code="200">The claim has been removed.</response>
        /// <response code="401">User has no permission.</response>
        /// <response code="500">Internal server error.</response>
        [ActionName("DeleteClaimFromRole")]
        [HttpDelete]
        [MapToApiVersion("1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteClaimFromRoleV1Async([FromBody] RoleClaimModel model)
        {
            if (!User.HasPermission(Claims.EditRoleClaims))
                return Unauthorized();

            if (model.claimType.Equals(Claims.Super.ToString()))
                return Unauthorized();

            if (!Enum.TryParse(model.claimType, out Claims claimType))
                return BadRequest("Invalid claim type.");

            ApplicationRole role = await mRoleManager.FindByIdAsync(model.roleId.ToString());
            if (role == null)
                return BadRequest("Unexistent Role.");

            ApplicationRoleClaim roleClaim =
                await mRoleManager.GetRoleClaimAsync(model.roleId, claimType.ToPermissionString());
            if (roleClaim == null)
                return Ok("Role doesn't owns claim.");

            Claim claim = new(roleClaim.ClaimType, roleClaim.ClaimValue);
            if ((await mRoleManager.RemoveClaimAsync(role, claim)).Succeeded)
                return Ok();
            return StatusCode(500);
        }

        /// <summary>
        ///     Define a new role for the user.
        /// </summary>
        /// <remarks>
        ///     This will add a role for the selected user.
        /// </remarks>
        /// <response code="200">The claim has been added.</response>
        /// <response code="401">User has no permission.</response>
        /// <response code="404">Target or Role not found.</response>
        /// <response code="500">Internal server error.</response>
        [ActionName("SetUserOnRole")]
        [HttpPut]
        [MapToApiVersion("1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetUserOnRoleV1Async([FromBody] RoleUserModel model)
        {
            if (!User.HasPermission(Claims.EditUserClaims))
                return Unauthorized();

            ApplicationUser user = null;
            if (!string.IsNullOrEmpty(model.userName))
                user = await mUserManager.FindByNameAsync(model.userName);
            else if (!string.IsNullOrEmpty(model.email))
                user = await mUserManager.FindByEmailAsync(model.userName);
            else
                user = await mUserManager.FindByIdAsync(model.userId.ToString());

            if (user == null)
                return NotFound("The selected user could not be found.");

            IList<string> roles = await mUserManager.GetRolesAsync(user);
            if (roles.Any(x => x.Equals(SuperAdministrator)) && !User.IsInRole(SuperAdministrator))
                return Forbid("The selected user is an Super Administrator and cannot be changed.");

            ApplicationRole selectedRole = await mRoleManager.FindByIdAsync(model.roleId.ToString());
            if (selectedRole == null)
                return NotFound("The selected role does not exist.");

            if (selectedRole.Name.Equals(SuperAdministrator) && !User.IsInRole(SuperAdministrator))
                return Forbid("You cannot set users on Super Administrator level.");

            if (roles.Any(x => x.Equals(selectedRole.Name)))
                return Conflict("You are trying to add the user on it's own role.");

            if (roles.Count > 0) await mUserManager.RemoveFromRolesAsync(user, roles);

            if (await mUserManager.AddToRoleAsync(user, selectedRole.Name) == IdentityResult.Success)
                return Ok(new
                {
                    user.Id,
                    user.UserName,
                    user.Email
                });

            return BadRequest();
        }

        /// <summary>
        ///     Removes an user from a role.
        /// </summary>
        /// <remarks>
        ///     This will remove the selected user from the role.
        /// </remarks>
        /// <response code="200">The role has been removed.</response>
        /// <response code="401">User has no permission.</response>
        /// <response code="404">Target or Role not found.</response>
        /// <response code="500">Internal server error.</response>
        [ActionName("DeleteUserFromRole")]
        [HttpDelete]
        [MapToApiVersion("1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUserFromRoleV1Async([FromBody] RoleUserModel model)
        {
            if (!User.HasPermission(Claims.EditUserClaims))
                return Unauthorized();

            ApplicationUser user = null;
            if (!string.IsNullOrEmpty(model.userName))
                user = await mUserManager.FindByNameAsync(model.userName);
            else if (!string.IsNullOrEmpty(model.email))
                user = await mUserManager.FindByEmailAsync(model.userName);
            else
                user = await mUserManager.FindByIdAsync(model.userId.ToString());

            if (user == null)
                return NotFound("The selected user could not be found.");

            IList<string> roles = await mUserManager.GetRolesAsync(user);
            if (roles.Any(x => x.Equals(SuperAdministrator)) && !User.IsInRole(SuperAdministrator))
                return Forbid("The selected user is an Super Administrator and cannot be changed.");

            ApplicationRole selectedRole = await mRoleManager.FindByIdAsync(model.roleId.ToString());
            if (selectedRole == null)
                return NotFound("The selected role does not exist.");

            if (await mUserManager.RemoveFromRoleAsync(user, selectedRole.Name) == IdentityResult.Success)
                return Ok();

            return BadRequest();
        }

        public class RoleUserModel
        {
            public int roleId { get; set; }
            public string userName { get; set; }
            public string email { get; set; }
            public int userId { get; set; }
        }

        public class RoleClaimModel
        {
            public int roleId { get; set; }
            public string claimType { get; set; }
        }
    }
}