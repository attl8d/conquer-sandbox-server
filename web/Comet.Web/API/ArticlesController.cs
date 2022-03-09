using Comet.Web.Model.Articles;
using Comet.Web.Services;
using Comet.Web.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comet.Web.API
{
    [ApiVersion("1")]
    [Route("[controller]/[action]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly ArticleService mService;
        private readonly UserManager mUserManager;

        public ArticlesController(ArticleService service, UserManager userManager)
        {
            mService = service;
            mUserManager = userManager;
        }

        /// <summary>
        /// Returns a list of results.
        /// </summary>
        /// <remarks>
        /// This will return a list with article category types. This should not contain much data, so this isn't
        /// paginated.
        /// </remarks>
        /// <response code="200">A list with the article category types.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet(Name = "GetActiveArticleCategoryTypes"), MapToApiVersion("1")]
        [ProducesResponseType(typeof(List<ArticleCategoryTypeModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetActiveArticleCategoryTypesV1Async()
        {
            return Ok(await mService.QueryActiveArticleCategoryTypeAsync());
        }

        /// <summary>
        /// Returns a list of results.
        /// </summary>
        /// <remarks>
        /// This will return a list with the article categories from a specific type. This should not contain much
        /// data, so this isn't paginated.
        /// </remarks>
        /// <param name="type">The category type to be exibhited.</param>
        /// <response code="200">A list with the article categories.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet(Name = "GetActiveArticleCategoriesByType"), MapToApiVersion("1")]
        [ProducesResponseType(typeof(List<ArticleCategoryModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetActiveArticleCategoriesByTypeV1Async(int type)
        {
            return Ok(await mService.QueryActiveArticleCategoriesByTypeAsync(type));
        }

        /// <summary>
        /// Returns a list of results.
        /// </summary>
        /// <remarks>
        /// This request will return a list with the articles for administration. It won't bring article content
        /// only data to be shown in a list for management.
        /// </remarks>
        /// <param name="categoryType">Article Category Type.</param>
        /// <param name="page">Current page index. [From 0]</param>
        /// <param name="ipp">Indexes per page.</param>
        /// <response code="200">A list with the articles.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet(Name = "GetByCategoryType"), MapToApiVersion("1")]
        [ProducesResponseType(typeof(List<AdminArticleListModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByCategoryTypeV1Async(int categoryType, int page = 0, int ipp = 10)
        {            
            return Ok(await mService.QueryByCategoryTypeAsync(categoryType, page, ipp));
        }

        /// <summary>
        /// Saves a comment for the given article.
        /// </summary>
        /// <remarks>
        /// This request will validate and save a comment for the given article. The user must be logged in and may not be banned.
        /// Filling the message with less than 6 characters or more than 1024 will return 400.
        /// </remarks>
        /// <returns>An object containing the comment data or error message.</returns>
        /// <response code="200">A payload containing the comment data.</response>
        /// <response code="204">An article with the given ID could not be found.</response>
        /// <response code="400">Something in the request isn't correct. Check the request payload.</response>
        /// <response code="401">Access denied.</response>
        /// <response code="500">Something went wrong.</response>
        [Authorize]
        [HttpPost(Name = "PostComment"), MapToApiVersion("1")]
        [ProducesResponseType(typeof(ArticleCommentResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ArticleCommentResultModel), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ArticleCommentResultModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ArticleCommentResultModel), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ArticleCommentResultModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostCommentV1Async([FromBody] PostCommentModel model)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            var user = await mUserManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var result = await mService.SaveCommentAsync(model.PostId, user.Id, model.Message, HttpContext.Connection.RemoteIpAddress.ToString(), Request.Cookies["ReadGuid"]);
            return StatusCode(result.StatusCode, result);
        }

        public class PostCommentModel
        {
            public int PostId { get; set; }
            public string Message { get; set; }
            public int Rating { get; set; }
        }
    }
}
