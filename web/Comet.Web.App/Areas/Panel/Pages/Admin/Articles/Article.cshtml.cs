using Comet.Web.App.Areas.Profile.Models;
using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Services;
using Comet.Web.App.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Comet.Web.App.Areas.Panel.Pages.Admin.Articles
{
    [Authorize]
    public class ArticleModel : PageModel
    {
        private readonly ApplicationDbContext mContext;
        private readonly UserManager mUserManager;
        private readonly SignInManager mSignInManager;

        public ArticleModel(ApplicationDbContext context, UserManager userManager, SignInManager signInManager)
        {
            mContext = context;
            mUserManager = userManager;
            mSignInManager = signInManager;
        }

        [BindProperty] public InputModel Input { get; set; }
        [BindProperty(SupportsGet = true)] public int ArticleId { get; set; }
        [BindProperty(SupportsGet = true)] public string Locale { get; set; }

        public string Author { get; private set; }

        public List<ArticleCategory> Categories { get; private set; } = new List<ArticleCategory>();
        public List<ArticleCategoryType> CategoryTypes { get; private set; } = new List<ArticleCategoryType>();

        public List<Languages> FeaturedLanguages { get; private set; } = new List<Languages>();
        public List<Languages> UnfeaturedLanguages { get; private set; } = new List<Languages>();

        public async Task LoadAsync()
        {
            CategoryTypes = await mContext.ArticleCategoryTypes.Where(x => x.Flags == 0).ToListAsync();

            var article = await mContext.Articles
                .Include(x => x.User)
                .Include(x => x.Category)
                .Include(x => x.Content).ThenInclude(x => x.Writer)
                .Include(x => x.Content).ThenInclude(x => x.LastEditor)
                .FirstOrDefaultAsync(x => x.Id == ArticleId);

            string locality = User.FindFirst(PersonalInfoType.Language.ToClaimString()).Value;
            if (article == null)
            {
                Input ??= new InputModel
                {
                    ArticleCategoryId = 0,
                    ArticleTypeId = 0,
                    Locale = locality ?? Thread.CurrentThread.CurrentUICulture.Name,
                    PublishDate = null
                };

                Author = User.FindFirst(PersonalInfoType.Name).Value;

                Categories = await mContext.ArticleCategories.Where(x => x.CategoryTypeId == CategoryTypes.Select(x => x.Id).Min()).ToListAsync();
            }
            else
            {
                var content = article.Content.FirstOrDefault(x => x.Locale.Equals(Locale, StringComparison.InvariantCultureIgnoreCase)) ?? article.Content.FirstOrDefault();
                Input ??= new InputModel
                {
                    ArticleCategoryId = article.CategoryId,
                    ArticleTypeId = article.Category.CategoryTypeId,
                    Locale = Locale.Equals(content?.Locale) ? content?.Locale : Locale,
                    Title = content?.Title,
                    Content = content?.Content,
                    PublishDate = article.PublishDate,
                    NotPublished = article.Flags.HasFlag(Article.ArticleFlags.NotPublished)
                };

                if (content?.Writer != null)
                {
                    var writer = await mUserManager.GetUserProfileAsync(content.Writer);
                    if (writer != null)
                        Author = writer.Name;
                }
                else
                {
                    Author = User.FindFirst(PersonalInfoType.Name).Value;
                }

                Categories = await mContext.ArticleCategories.Where(x => x.CategoryTypeId == Input.ArticleTypeId).ToListAsync();
            }

            foreach (var language in LanguageService.AvailableLanguages)
            {
                if (article?.Content.Any(x => x.Locale.Equals(language.LanguageCultureName)) == true)
                {
                    FeaturedLanguages.Add(language);
                }
                else
                {
                    UnfeaturedLanguages.Add(language);
                }
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.HasPermission(Security.Claims.CanWriteArticles))
            {
                return Forbid();
            }

            await LoadAsync();

            if (Input == null)
            {
                return RedirectToPage("/Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!User.HasPermission(Security.Claims.CanWriteArticles))
            {
                return Forbid();
            }

            await LoadAsync();
            if (!ModelState.IsValid)
            {
                
                return Page();
            }

            await mContext.Database.BeginTransactionAsync();

            try
            {
                var user = await mUserManager.GetUserAsync(User);
                var article = await mContext.Articles
                    .Include(x => x.User)
                    .Include(x => x.Category)
                    .Include(x => x.Content).ThenInclude(x => x.Writer)
                    .Include(x => x.Content).ThenInclude(x => x.LastEditor)
                    .FirstOrDefaultAsync(x => x.Id == ArticleId);
                ArticleContent content = null;
                if (article != null)
                {
                    content = article.Content.FirstOrDefault(x => x.Locale.Equals(Input.Locale, StringComparison.InvariantCultureIgnoreCase));
                }
                else
                {
                    article = new Article
                    {
                        CategoryId = Input.ArticleCategoryId,
                        CreationDate = DateTime.Now,
                        Flags = Article.ArticleFlags.Visible,
                        UserId = user.Id
                    };

                    if (Input.NotPublished)
                    {
                        article.Flags |= Article.ArticleFlags.NotPublished;
                    }
                    else
                    {
                        //article.PublishDate = DateTime.Now;
                        article.PublishDate = Input.PublishDate ?? DateTime.Now;
                    }
                }

                if (content != null)
                {
                    content.Title = Input.Title;
                    content.Content = Input.Content;
                    content.LastEditorId = user.Id;
                }
                else
                {
                    content = new ArticleContent
                    {
                        Title = Input.Title,
                        Content = Input.Content,
                        WriterId = user.Id,
                        Locale = Input.Locale
                    };
                    article.Content ??= new List<ArticleContent>();
                    article.Content.Add(content);
                }

                mContext.Update(article);
                await mContext.SaveChangesAsync();
                await mContext.Database.CommitTransactionAsync();
                
                ArticleId = article.Id;
            }
            catch (Exception ex)
            {
                await mContext.Database.RollbackTransactionAsync();
                return StatusCode(500, ex.Message);
            }
            
            return RedirectToPage(new
            {
                ArticleId = ArticleId,
                Locale = Input.Locale
            });
        }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            public string Title { get; set; }
            [Required]
            [DataType(DataType.MultilineText)]
            public string Content { get; set; }
            [Required]
            [DataType(DataType.Text)]
            public string Locale { get; set; }
            [Required]
            public int ArticleTypeId { get; set; }
            [Required]
            public int ArticleCategoryId { get; set; }
            [DataType(DataType.DateTime)]
            public DateTime? PublishDate { get; set; }
            [Required]
            public bool NotPublished { get; set; }
        }
    }
}
