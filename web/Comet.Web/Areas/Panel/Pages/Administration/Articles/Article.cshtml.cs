using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Comet.Web.Areas.Profile.Models;
using Comet.Web.Data.Entities;
using Comet.Web.Model.Articles;
using Comet.Web.Model.Identity;
using Comet.Web.Services;
using Comet.Web.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.FileSystemGlobbing.Internal;

namespace Comet.Web.Areas.Panel.Pages.Administration.Articles
{
    [Authorize]
    public class ArticleModel : PageModel
    {
        private readonly ArticleService mService;
        private readonly UserManager mUserManager;
        private readonly SignInManager mSignInManager;
        private readonly LanguageService mLanguageService;

        public ArticleModel(ArticleService service, UserManager userManager, SignInManager signInManager, LanguageService languageService)
        {
            mService = service;
            mUserManager = userManager;
            mSignInManager = signInManager;
            mLanguageService = languageService;
        }

        [BindProperty] public InputModel Input { get; set; }
        [BindProperty(SupportsGet = true)] public int ArticleId { get; set; }
        [BindProperty(SupportsGet = true)] public string Locale { get; set; }

        public string Author { get; private set; }

        public List<ArticleCategoryModel> Categories { get; private set; } = new();
        public List<ArticleCategoryTypeModel> CategoryTypes { get; private set; } = new();

        public List<Languages> FeaturedLanguages { get; private set; } = new();
        public List<Languages> UnfeaturedLanguages { get; private set; } = new();

        private async Task LoadAsync()
        {
            CategoryTypes = await mService.QueryActiveArticleCategoryTypeAsync();

            Article article = await mService.QueryFullArticleByIdAsync(ArticleId);

            string locale = User.FindFirst(CustomClaimTypes.Language)?.Value ?? mLanguageService.CurrentLocale;
            if (article == null)
            {
                Input ??= new InputModel
                {
                    ArticleCategoryId = 0,
                    ArticleTypeId = 0,
                    Locale = locale,
                    PublishDate = null
                };

                Author = User.FindFirst(ClaimTypes.Name)?.Value;

                Categories = await mService.QueryActiveArticleCategoriesByTypeAsync(CategoryTypes.Select(x => x.Id).Min());
            }
            else
            {
                var content =
                    article.Content.FirstOrDefault(x => x.Locale.Equals(locale, StringComparison.InvariantCultureIgnoreCase))
                    ?? article.Content.FirstOrDefault();

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
                    ClaimsPrincipal principal = await mSignInManager.CreateUserPrincipalAsync(content.Writer);
                    ProfileModel writer = await mUserManager.GetUserProfileAsync(content.Writer, principal);
                    if (writer != null)
                        Author = writer.Name;
                }
                else
                {
                    Author = User.FindFirst(ClaimTypes.Name)?.Value;
                }

                Categories = await mService.QueryActiveArticleCategoriesByTypeAsync(Input.ArticleTypeId);
            }

            foreach (Languages language in LanguageService.AvailableLanguages)
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

            ApplicationUser user = await mUserManager.GetUserAsync(User);
            (int Result, int ArticleId) result = await mService.PostArticleAsync(user, ArticleId, Input);

            if (result.Result != StatusCodes.Status200OK)
            {
                ModelState.AddModelError("CouldNotSave", mLanguageService.GetString("ErrorSavingArticle"));
                return Page();
            }

            return RedirectToPage(new
            {
                result.ArticleId, Input.Locale
            });
        }

        public class InputModel
        {
            [Required] [DataType(DataType.Text)] public string Title { get; set; }

            [Required]
            [DataType(DataType.MultilineText)]
            public string Content { get; set; }

            [Required] [DataType(DataType.Text)] public string Locale { get; set; }
            [Required] public int ArticleTypeId { get; set; }
            [Required] public int ArticleCategoryId { get; set; }
            [DataType(DataType.DateTime)] public DateTime? PublishDate { get; set; }
            [Required] public bool NotPublished { get; set; }
            [Required] public bool NotListed { get; set; }
        }
    }
}