using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Comet.Web.Areas.Panel.Pages.Administration.Articles;
using Comet.Web.Areas.Profile.Models;
using Comet.Web.Data;
using Comet.Web.Data.Entities;
using Comet.Web.Model.Articles;
using Comet.Web.Services.Identity;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Comet.Web.Services
{
    public sealed class ArticleService
    {
        private const int CommentsIpp = 25;

        private readonly ApplicationDbContext mContext;
        private readonly IHttpContextAccessor mHttpContextAccessor;
        private readonly LanguageService mLanguage;
        private readonly SignInManager mSignInManager;
        private readonly UserManager mUserManager;

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

        public Task<List<Article>> GetAsync(int type, int page = 0, int ipp = 5)
        {
            return mContext
                   .Articles
                   .Where(x => x.Category.CategoryTypeId == type && x.PublishDate <= DateTime.Now && x.Flags == Article.ArticleFlags.Visible)
                   .Include(x => x.Category)
                   .Include(x => x.User)
                   .Include(x => x.Content).ThenInclude(x => x.Writer)
                   .Include(x => x.Content).ThenInclude(x => x.LastEditor)
                   .Include(x => x.Category).ThenInclude(x => x.CategoryType)
                   .OrderByDescending(x => x.Id)
                   .Skip(page * ipp)
                   .Take(ipp)
                   .ToListAsync();
        }

        public Task<List<Article>> GetByCategoryAsync(int type, int page, int ipp = 5)
        {
            return mContext
                   .Articles
                   .Where(x => x.CategoryId == type && x.PublishDate <= DateTime.Now && x.Flags == Article.ArticleFlags.Visible)
                   .Include(x => x.Category)
                   .Include(x => x.User)
                   .Include(x => x.Content).ThenInclude(x => x.Writer)
                   .Include(x => x.Content).ThenInclude(x => x.LastEditor)
                   .Include(x => x.Category).ThenInclude(x => x.CategoryType)
                   .OrderByDescending(x => x.Id)
                   .Skip(page * ipp)
                   .Take(ipp)
                   .ToListAsync();
        }

        public Task<Article> QueryArticleByIdAsync(int articleId)
        {
            return mContext.Articles.FirstOrDefaultAsync(x => x.Id == articleId);
        }

        public Task<Article> QueryFullArticleByIdAsync(int articleId)
        {
            return mContext.Articles
                    .Include(x => x.User)
                    .Include(x => x.Category)
                    .Include(x => x.Content).ThenInclude(x => x.Writer)
                    .Include(x => x.Content).ThenInclude(x => x.LastEditor)
                    .Include(x => x.Category).ThenInclude(x => x.CategoryType)
                    .FirstOrDefaultAsync(x => x.Id == articleId);
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
            return mContext.ArticleCategories.Where(x => x.CategoryTypeId == type && x.Flags == 0).Select(
                x => new ArticleCategoryModel
                {
                    Id = x.Id,
                    Name = mLanguage.GetString(x.Name),
                    RealName = x.Name
                }).ToListAsync();
        }

        public Task<List<ArticleCategory>> QueryActiveArticleCategoriesByTypeCompleteAsync(int type)
        {
            return mContext.ArticleCategories.Where(x => x.CategoryTypeId == type)
                           .OrderBy(x => x.Order)
                           .Include(x => x.Articles)
                           .ThenInclude(y => y.Content)
                           .ToListAsync();
        }

        public async Task<List<AdminArticleListModel>> QueryByCategoryTypeAsync(
            int categoryType, int page = 0, int ipp = 10)
        {
            List<ArticleCategory> categories = await mContext
                                                     .ArticleCategories.AsQueryable()
                                                     .Where(x => x.CategoryTypeId == categoryType)
                                                     .Include(x => x.Articles).ThenInclude(x => x.Content)
                                                     .Include(x => x.Articles).ThenInclude(x => x.User)
                                                     .OrderByDescending(x => x.Id)
                                                     .Skip(Math.Max(0, page - 1))
                                                     .Take(ipp <= 0 ? int.MaxValue : ipp)
                                                     .ToListAsync();

            List<AdminArticleListModel> result = new();
            foreach (ArticleCategory cat in categories)
            foreach (Article article in cat.Articles)
            {
                ProfileModel profile =
                    await mUserManager.GetUserProfileAsync(article.User, mHttpContextAccessor.HttpContext.User);
                ArticleContent content =
                    article.Content.FirstOrDefault(x => x.Locale.Equals(mLanguage.CurrentLocale)) ??
                    article.Content.FirstOrDefault();

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

            return result;
        }

        public async Task<ArticleCommentResultModel> SaveCommentAsync(int articleId, int senderId, string message,
                                                                      string ipAddress = "0.0.0.0", string tokenId = "")
        {
            Task<ApplicationUser> user = mUserManager.FindByIdAsync(senderId.ToString());
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

            var html = new HtmlDocument();
            html.LoadHtml(message);
            message = html.DocumentNode.InnerText;

            var comment = new ArticleComment
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

        public async Task<int> RemoveCommentAsync(int commentId)
        {
            ArticleComment comment = await mContext.ArticleComments.FirstOrDefaultAsync(x => x.Id == commentId);
            if (comment == null || (comment.Flag & 0x1) != 0)
                return StatusCodes.Status204NoContent;

            comment.Flag |= 1;
            await mContext.SaveAsync(comment);
            return StatusCodes.Status200OK;
        }

        public async Task<(int Result, int ArticleId)> PostArticleAsync(ApplicationUser user, int articleId, ArticleModel.InputModel model)
        {
            var article = await QueryFullArticleByIdAsync(articleId);
            try
            {
                await mContext.Database.BeginTransactionAsync();

                ArticleContent content = null;
                if (article != null)
                {
                    content = article.Content.FirstOrDefault(x => x.Locale.Equals(model.Locale, StringComparison.InvariantCultureIgnoreCase));
                }
                else
                {
                    article = new Article
                    {
                        CategoryId = model.ArticleCategoryId,
                        CreationDate = DateTime.Now,
                        Flags = Article.ArticleFlags.Visible,
                        UserId = user.Id
                    };

                    if (model.NotPublished)
                    {
                        article.Flags |= Article.ArticleFlags.NotPublished;
                    }
                    else
                    {
                        article.PublishDate = model.PublishDate ?? DateTime.Now;
                    }
                }

                if (content != null)
                {
                    content.Title = model.Title;
                    content.Content = model.Content;
                    content.LastEditorId = user.Id;
                }
                else
                {
                    content = new ArticleContent
                    {
                        Title = model.Title,
                        Content = model.Content,
                        WriterId = user.Id,
                        Locale = model.Locale
                    };
                    article.Content ??= new List<ArticleContent>();
                    article.Content.Add(content);
                }

                await mContext.SaveAsync(article);
                await mContext.Database.CommitTransactionAsync();
            }
            catch
            {
                await mContext.Database.RollbackTransactionAsync();
                return (StatusCodes.Status500InternalServerError, 0);
            }
            return (StatusCodes.Status200OK, article.Id);
        }

        public Task SaveArticleAsync(Article article) => mContext.SaveAsync(article);

        public Task<ArticleReadToken> QueryUserReadTokenAsync(int articleId, string readToken)
        {
            return mContext.ArticleReadTokens.FirstOrDefaultAsync(x => x.ArticleId == articleId && x.TokenId.Equals(readToken));
        }

        public Task SaveReadUserTokenAsync(ArticleReadToken token) => mContext.SaveAsync(token);

        public Task<List<ArticleComment>> QueryArticleCommentsAsync(int articleId, int page) => mContext.ArticleComments
                                                                   .Include(x => x.User)
                                                                   .Include(x => x.Article)
                                                                   .Where(x => x.ArticleId == articleId)
                                                                   .OrderBy(x => x.Id)
                                                                   .Skip(page * CommentsIpp)
                                                                   .Take(CommentsIpp)
                                                                   .ToListAsync();

        public async Task<List<Article>> QueryMostPopularArticlesAsync(int num)
        {
            DataTable articles = await mContext.SelectAsync($"SELECT COUNT(1), ArticleId FROM article_read_token GROUP BY ArticleId ORDER BY 1 DESC LIMIT {num};");
            List<Article> result = new();

            foreach (DataRow row in articles.Rows)
            {
                int idArticle = int.Parse(row["ArticleId"]?.ToString() ?? "0");
                Article article = await QueryFullArticleByIdAsync(idArticle);
                if (article != null)
                    result.Add(article);
            }

            if (result.Count < num)
            {
                int amount = num - result.Count;
                result.AddRange(await mContext.Articles
                                              .Where(x => result.Select(y => y.Id).All(y => y != x.Id))
                                              .Include(x => x.User)
                                              .Include(x => x.Category)
                                              .Include(x => x.Content).ThenInclude(x => x.Writer)
                                              .Include(x => x.Content).ThenInclude(x => x.LastEditor)
                                              .Include(x => x.Category).ThenInclude(x => x.CategoryType)
                                              .Take(amount)
                                              .OrderByDescending(x => x.Views).ToListAsync());
            }

            return result;
        }
    }
}