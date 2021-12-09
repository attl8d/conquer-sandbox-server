using Comet.Web.App.Areas.Profile.Models;
using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Services;
using Comet.Web.App.Services.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Web.App.Areas.Read.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext mContext;
        private readonly UserManager mUserManager;
        private readonly SignInManager mSignInManager;
        private readonly LanguageService mLanguage;

        public IndexModel(ApplicationDbContext context, UserManager userManager, SignInManager signInManager, LanguageService language)
        {
            mContext = context;
            mUserManager = userManager;
            mSignInManager = signInManager;
            mLanguage = language;
        }

        [BindProperty(SupportsGet = true)]
        public int ArticleId { get; set; }
        public string Author { get; private set; }
        public Article Article { get; private set; }
        public ArticleContent ArticleContent { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var article = await mContext.Articles
                .Include(x => x.User)
                .Include(x => x.Category)
                .Include(x => x.Content).ThenInclude(x => x.Writer)
                .Include(x => x.Content).ThenInclude(x => x.LastEditor)
                .FirstOrDefaultAsync(x => x.Id == ArticleId);

            if (article == null || article.Content == null || article.Content.Count <= 0)
            {
                return Redirect("/");
            }

            string locality = User.FindFirst(PersonalInfoType.Language.ToClaimString())?.Value ?? mLanguage.CurrentLocale;

            Article = article;
            ArticleContent = article.Content.FirstOrDefault(x => x.Locale.Equals(locality, StringComparison.InvariantCultureIgnoreCase)) ?? article.Content.FirstOrDefault();

            if (ArticleContent?.Writer != null)
            {
                var writer = await mUserManager.GetUserProfileAsync(ArticleContent.Writer);
                if (writer != null)
                    Author = writer.Name;
            }

            return Page();
        }
    }
}
