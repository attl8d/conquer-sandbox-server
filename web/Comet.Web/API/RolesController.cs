using Comet.Web.Areas.Panel.Models;
using Comet.Web.Data.Entities;
using Comet.Web.Services;
using Comet.Web.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static Comet.Web.Services.Security;

namespace Comet.Web.API
{
    [ApiVersion("1")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Policy = Administrator)]
    public class RolesController : ControllerBase
    {
        private readonly UserManager mUserManager;
        private readonly LanguageService mLanguage;
        private readonly SignInManager mSigninManager;
        private readonly RoleManager mRoleManager;

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
        /// Returns a list of roles.
        /// </summary>
        /// <remarks>
        /// This will return a paginated query of all registered roles at the moment.
        /// </remarks>
        /// <param name="page">The index of the page.</param>
        /// <param name="ipp">Indexes per page.</param>
        /// <response code="200">A list with the registered roles.</response>
        /// <response code="401">User has no permission.</response>
        /// <response code="500">Internal server error.</response>
        [ActionName("")]
        [HttpGet, MapToApiVersion("1")]
        [ProducesResponseType(typeof(List<AdminRoleListModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync(int page, int ipp = 10)
        {
            if (!User.HasPermission(Claims.ViewRoles))
                return Unauthorized();

            var roles = await mRoleManager.GetRolesAsync(page, ipp);
            var result = new List<AdminRoleListModel>();
            foreach (var role in roles)
            {
                result.Add(new AdminRoleListModel
                {
                    Id = role.Id,
                    Name = role.Name,
                    CreationDate = role.CreationDate,
                    ModifiedDate = role.ModifiedDate,
                    NormalizedName = role.NormalizedName,
                    Count = (await mUserManager.GetUsersInRoleAsync(role.Name)).Count
                });
            }
            return Ok(result);
        }

        /// <summary>
        /// Returns a list of users.
        /// </summary>
        /// <remarks>
        /// This will return a paginated query of all registered users in specific role at the moment.
        /// </remarks>
        /// <param name="roleId">The identity of the role.</param>
        /// <param name="page">The index of the page.</param>
        /// <param name="ipp">Indexes per page.</param>
        /// <response code="200">A list with the registered roles.</response>
        /// <response code="401">User has no permission.</response>
        /// <response code="500">Internal server error.</response>
        [ActionName("GetUsersByRole")]
        [HttpGet, MapToApiVersion("1")]
        [ProducesResponseType(typeof(List<AdminUserListModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsersByRoleV1Async(int roleId, int page = 0, int ipp = 10)
        {
            if (!User.HasPermission(Claims.ViewRoles))
                return Forbid();

            var users = await mUserManager.GetUsersByRoleAsync(roleId, page, ipp);
            var result = users.Select(x => new AdminUserListModel
            {
                Id = x.Id,
                Email = x.Email,
                UserName = x.UserName,
                CreationDate = x.CreationDate
            }).ToList();
            return Ok(result);
        }

        /// <summary>
        /// Returns a list of claims.
        /// </summary>
        /// <remarks>
        /// This will return a list of all registered claims for a specific role at the moment.
        /// </remarks>
        /// <param name="roleId">The identity of the role.</param>
        /// <response code="200">A list with the registered roles.</response>
        /// <response code="204">Invalid role ID.</response>
        /// <response code="401">User has no permission.</response>
        /// <response code="500">Internal server error.</response>
        [ActionName("GetClaimsByRole")]
        [HttpGet, MapToApiVersion("1")]
        [ProducesResponseType(typeof(List<AdminRoleClaimModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetClaimsByRoleV1Async(int roleId)
        {
            if (!User.HasPermission(Claims.ViewClaims))
                return Unauthorized();

            var role = await mRoleManager.FindByIdAsync(roleId.ToString());
            if (role == null)
                return NoContent();

            var claims = await mRoleManager.GetClaimsAsync(role);
            var result = new List<AdminRoleClaimModel>();
            foreach (var claim in claims)
            {
                result.Add(new AdminRoleClaimModel
                {
                    ClaimType = claim.Type.Split(new char[] { '.' }, 2)[1],
                    ClaimValue = claim.Value,
                    ClaimName = (Enum.TryParse<Claims>(claim.Type, out var eClaim) ? mLanguage.GetString(eClaim.ToPermissionString()) : mLanguage.GetString(claim.Type))
                });
            }
            return Ok(result);
        }

        /// <summary>
        /// Insert a new claim.
        /// </summary>
        /// <remarks>
        /// This will insert a new claim for the required role. User claims override role claims.
        /// </remarks>
        /// <response code="200">The claim has been added.</response>
        /// <response code="401">User has no permission.</response>
        /// <response code="500">Internal server error.</response>
        [ActionName("SetClaimToRole")]
        [HttpPut, MapToApiVersion("1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]        
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetClaimToRoleAsync([FromBody] RoleClaimModel model)
        {
            if (!User.HasPermission(Claims.EditRoleClaims))
                return Unauthorized();

            if (model.claimType.Equals(Claims.Super.ToString()))
                return Unauthorized();

            if (!Enum.TryParse(model.claimType, out Claims claimType))
                return BadRequest("Invalid claim type.");

            var role = await mRoleManager.FindByIdAsync(model.roleId.ToString());
            if (role == null)
                return BadRequest("Unexistent Role.");

            var roleClaim = await mRoleManager.GetRoleClaimAsync(model.roleId, claimType.ToPermissionString());
            if (roleClaim != null)
                return Ok("Role already owns claim.");

            Claim claim = new (claimType.ToPermissionString(), "1");            
            if ((await mRoleManager.AddClaimAsync(role, claim)).Succeeded)
                return Ok(claim);

            return StatusCode(500);
        }

        /// <summary>
        /// Delete a claim.
        /// </summary>
        /// <remarks>
        /// This will remove a claim from a role. User claims override role claims.
        /// </remarks>
        /// <response code="200">The claim has been removed.</response>
        /// <response code="401">User has no permission.</response>
        /// <response code="500">Internal server error.</response>
        [ActionName("DeleteClaimFromRole")]
        [HttpDelete, MapToApiVersion("1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteClaimFromRoleAsync([FromBody] RoleClaimModel model)
        {
            if (!User.HasPermission(Claims.EditRoleClaims))
                return Unauthorized();

            if (model.claimType.Equals(Claims.Super.ToString()))
                return Unauthorized();

            if (!Enum.TryParse(model.claimType, out Claims claimType))
                return BadRequest("Invalid claim type.");

            var role = await mRoleManager.FindByIdAsync(model.roleId.ToString());
            if (role == null)
                return BadRequest("Unexistent Role.");

            var roleClaim = await mRoleManager.GetRoleClaimAsync(model.roleId, claimType.ToPermissionString());
            if (roleClaim == null)
                return Ok("Role doesn't owns claim.");

            Claim claim = new(roleClaim.ClaimType, roleClaim.ClaimValue);
            if ((await mRoleManager.RemoveClaimAsync(role, claim)).Succeeded)
                return Ok();
            return StatusCode(500);
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
