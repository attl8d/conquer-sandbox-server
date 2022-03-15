using System.Threading.Tasks;
using Comet.Web.Services;
using Comet.Web.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comet.Web.API
{
    [ApiVersion("1")]
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ConquerGameController : ControllerBase
    {
        private readonly UserManager mUserManager;
        private readonly ConquerGameService mGameService;

        public ConquerGameController(UserManager userManager, ConquerGameService gameService)
        {
            mUserManager = userManager;
            mGameService = gameService;
        }

        [ActionName("QueryDeletedCharacters")]
        [HttpGet]
        [MapToApiVersion("1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDeletedCharactersV1Async(string realmName)
        {
            var result = await mGameService.QueryDeletedCharactersAsync(realmName);
            if (result == null)
                return NoContent();
            return Ok(result);
        }
    }
}
