using System.Threading.Tasks;
using Comet.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comet.Web.API
{
    /// <summary>
    ///     This controller executes jobs to automate data synchronization between the game servers and
    ///     the web data.
    /// </summary>
    [ApiVersion("1")]
    [Route("427EA4B2-BE46-4E30-B2FF-C8100AEC0DBF/[action]")]
    [ApiController]
    public class DailyUpdateController : ControllerBase
    {
        private readonly GameServerDataService mDataService;

        public DailyUpdateController(GameServerDataService dataService)
        {
            mDataService = dataService;
        }

        /// <summary>
        ///     Query for user data.
        /// </summary>
        /// <remarks>
        ///     Looks for user data in the active game servers and synchronize them with the web server
        ///     for displaying user rank data and etc. Requires no data or parameters to be executed.
        /// </remarks>
        /// <returns>A successfull message (or not).</returns>
        /// <response code="200">With a "Executed" string if success.</response>
        /// <response code="500">Something went wrong.</response>
        [HttpGet]
        [MapToApiVersion("1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshGameServerDataV1Async()
        {
            await mDataService.ExecuteMigrationAsync();
            return Ok("Executed");
        }
    }
}