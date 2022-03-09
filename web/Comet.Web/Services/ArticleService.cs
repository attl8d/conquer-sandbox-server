using Comet.Web.Data;
using Comet.Web.Data.Entities;
using Comet.Web.Model.Articles;
using Comet.Web.Services.Identity;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Web.Services
{
    public sealed class ArticleService
    {
        private readonly ApplicationDbContext mContext;
        private readonly LanguageService mLanguage;
        private readonly UserManager mUserManager;
        private readonly SignInManager mSignInManager;
        private readonly IHttpContextAccessor mHttpContextAccessor;

        public ArticleService(ApplicationDbContext context,
            LanguageService language,
            UserManager userManager, 
            SignInManager signInManager,
            IHttpContextAccessor httpContextAccessor)
        {
            mContext = context;
            mLanguage = language;
            mUserManager = userManager;
            mSignInManager = signInManager;
            mHttpContextAccessor = httpContextAccessor;
        }

        public Task<List<Article>> GetAsync(int page = 0, int ipp = 5)
        {
            return mContext
                .Articles
                .Where(x => x.PublishDate <= DateTime.Now)
                .Include(x => x.Category)
                .Include(x => x.User)
                .Include(x => x.Content).ThenInclude(x => x.Writer)
                .Include(x => x.Content).ThenInclude(x => x.LastEditor)
                .OrderByDescending(x => x.Id)
                .Take(5)
                .ToListAsync();
        }

        public Task<Article> QueryArticleByIdAsync(int articleId)
        {
            return mContext.Articles.FirstOrDefaultAsync(x => x.Id == articleId);
        }

        public Task<List<ArticleCategoryTypeModel>> QueryActiveArticleCategoryTypeAsync()
        {
            return mContext.ArticleCategoryTypes.Where(x => x.Flags == 0).Select(x => new ArticleCategoryTypeModel
            {
                Id = x.Id,
                Name = mLanguage.GetString(x.Name)
            }).ToListAsync();
        }

        public Task<List<ArticleCategoryModel>> QueryActiveArticleCategoriesByTypeAsync(int type)
        {
            return mContext.ArticleCategories.Where(x => x.CategoryTypeId == type && x.Flags == 0).Select(x => new ArticleCategoryModel
            {
                Id = x.Id,
                Name = mLanguage.GetString(x.Name)
            }).ToListAsync();
        }

        public async Task<List<AdminArticleListModel>> QueryByCategoryTypeAsync(int categoryType, int page = 0, int ipp = 10)
        {
            var categories = await mContext.ArticleCategories.AsQueryable().Where(x => x.CategoryTypeId == categoryType)
                .Include(x => x.Articles).ThenInclude(x => x.Content)
                .Include(x => x.Articles).ThenInclude(x => x.User)
                .OrderByDescending(x => x.Id)
                .Skip(Math.Max(0, page - 1))
                .Take(ipp <= 0 ? int.MaxValue : ipp)
                .ToListAsync();

            List<AdminArticleListModel> result = new();
            foreach (var cat in categories)
            {
                foreach (var article in cat.Articles)
                {
                    var profile = await mUserManager.GetUserProfileAsync(article.User, mHttpContextAccessor.HttpContext.User);
                    var content = article.Content.FirstOrDefault(x => x.Locale.Equals(mLanguage.CurrentLocale)) ?? article.Content.FirstOrDefault();

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

            return result;
        }

        public async Task<ArticleCommentResultModel> SaveCommentAsync(int articleId, int senderId, string message, string ipAddress = "0.0.0.0", string tokenId = "")
        {
            var user = mUserManager.FindByIdAsync(senderId.ToString());
            if (user == null)
                return new ArticleCommentResultModel
                {
                    ErrorMessage = mLanguage.GetString("Unauthorized"),
                    StatusCode = StatusCodes.Status401Unauthorized
                };

            Article article = await QueryArticleByIdAsync(articleId);
            if (article == null)
                return new ArticleCommentResultModel
                {
                    ErrorMessage = mLanguage.GetString("NoContent"),
                    StatusCode = StatusCodes.Status204NoContent
                };

            if (string.IsNullOrEmpty(message) || message.Length < 6 || message.Length > 1024)
                return new ArticleCommentResultModel
                {
                    ErrorMessage = mLanguage.GetString("CommentMessageSize", 6, 1024),
                    StatusCode = StatusCodes.Status400BadRequest
                };

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(message);
            message = html.DocumentNode.InnerText;

            ArticleComment comment = new ArticleComment
            {
                ArticleId = articleId,
                CreationDate = DateTime.Now,
                Dislikes = 0,
                Likes = 0,
                IpAddress = ipAddress,
                TokenId = tokenId,
                Flag = 0,
                Message = message,
                UserId = senderId
            };
            if (!await mContext.SaveAsync(comment))
                return null;

            return new ArticleCommentResultModel
            {
                Id = comment.Id,
                Message = message,
                CreatorName = "",
                Likes = 0,
                Dislikes = 0,
                CreationDate = comment.CreationDate,
                UpdateDate = comment.UpdateDate,

                StatusCode = StatusCodes.Status201Created
            };
        }
    }
}
