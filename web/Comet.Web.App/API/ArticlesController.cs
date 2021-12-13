using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Services;
using Comet.Web.App.Services.Identity;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Web.App.API
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly ApplicationDbContext mContext;
        private readonly LanguageService mLanguage;
        private readonly UserManager mUserManager;
        private readonly SignInManager mSignInManager;

        public ArticlesController(ApplicationDbContext context, LanguageService language, UserManager userManager, SignInManager signInManager)
        {
            mContext = context;
            mLanguage = language;
            mUserManager = userManager;
            mSignInManager = signInManager;
        }

        [HttpGet(Name = "GetActiveArticleCategoryTypes")]
        public async Task<IActionResult> GetActiveArticleCategoryTypesAsync()
        {
            var result = await mContext.ArticleCategoryTypes.Where(x => x.Flags == 0).ToListAsync();
            return Ok(result.Select(x => new
            {
                x.Id,
                Name = mLanguage.GetString(x.Name)
            }));
        }

        [HttpGet(Name = "GetActiveArticleCategoriesByType")]
        public async Task<IActionResult> GetActiveArticleCategoriesByTypeAsync(int type)
        {
            var result = await mContext.ArticleCategories.Where(x => x.CategoryTypeId == type && x.Flags == 0).ToListAsync();
            return Ok(result.Select(x => new
            {
                x.Id,
                Name = mLanguage.GetString(x.Name)
            }));
        }

        [HttpGet(Name = "GetByCategoryType")]
        public async Task<IActionResult> GetByCategoryTypeAsync(int categoryType, int page = 0,
            int ipp = 10)
        {
            var categories = await mContext.ArticleCategories.Where(x => x.CategoryTypeId == categoryType)
                .Include(x => x.Articles).ThenInclude(x => x.Content)
                .Include(x => x.Articles).ThenInclude(x => x.User)
                .OrderByDescending(x => x.Id)
                .Skip(Math.Max(0, page - 1))
                .Take(ipp)
                .ToListAsync();

            List<AdminArticleListModel> result = new List<AdminArticleListModel>();
            foreach (var cat in categories)
            {
                foreach (var article in cat.Articles)
                {
                    var profile = await mUserManager.GetUserProfileAsync(article.User);

                    var content = article.Content.FirstOrDefault(x => 
                        x.Locale.Equals(profile.PersonalInfo.FirstOrDefault(
                            x => x.Key == Areas.Profile.Models.PersonalInfoType.Locality).Value)) ?? article.Content.FirstOrDefault();

                    result.Add(new AdminArticleListModel
                    {
                        Id = article.Id,
                        Writer = profile.Name,
                        Category = mLanguage.GetString(cat.Name),
                        Title = content.Title,
                        PublishDate = article.PublishDate.ToString("O"),
                        Locales = new List<string>(article.Content.Select(x => x.Locale))
                    });
                }
            }

            return Ok(result);
        }

        [HttpPost(Name = "PostComment")]
        public async Task<IActionResult> PostCommentAsync([FromBody] PostCommentModel model)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            var user = await mUserManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            Article article = await mContext.Articles.FirstOrDefaultAsync(x => x.Id == model.PostId);
            if (article == null)
                return NoContent();

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(model.Message);
            string message = html.DocumentNode.InnerText;

            if (message.Length < 6 || message.Length > 1024)
                return BadRequest(mLanguage.GetString("CommentMessageSize", 6, 1024));

            ArticleComment comment = new ArticleComment
            {
                ArticleId = model.PostId,
                CreationDate = DateTime.Now,
                Dislikes = 0,
                Likes = 0,
                IpAddress = HttpContext.Connection.RemoteIpAddress.ToString(),
                TokenId = Request.Cookies["ReadGuid"],
                Flag = 0,
                Message = message,
                UserId = user.Id
            };

            if (!await mContext.SaveAsync(comment))
                return UnprocessableEntity("DatabaseError");

            return Ok();
        }

        public class PostCommentModel
        {
            public int PostId { get; set; }
            public string Message { get; set; }
            public int Rating { get; set; }
        }

        public class NewsDisplayModel
        {
            public int Id { get; set; }
            public string WriterId { get; set; }
            public string Writer { get; set; }
            public string Title { get; set; }
            public string Preview { get; set; }
            public string ThumbnailUrl { get; set; }
        }

        public class AdminArticleListModel
        {
            public int Id { get; set; }
            public string Writer { get; set; }
            public string Category { get; set; }
            public string Title { get; set; }
            public string PublishDate { get; set; }
            public List<string> Locales { get; set; } = new List<string>();
        }
    }
}
