using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Comet.Shared;
using Comet.Web.Areas.Profile.Models;
using Comet.Web.Data.Entities;
using Comet.Web.Model.Identity;
using Comet.Web.Services;
using Comet.Web.Services.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Comet.Web.Areas.Guides.Pages
{
    public class ReadModel : PageModel
    {
        private readonly UserManager mUserManager;
        private readonly LanguageService mLanguage;
        private readonly ArticleService mService;
        private readonly SignInManager mSignInManager;

        public ReadModel(ArticleService service,
                         UserManager userManager,
                         LanguageService language,
                         SignInManager signInManager)
        {
            mService = service;
            mUserManager = userManager;
            mLanguage = language;
            mSignInManager = signInManager;
        }

        [BindProperty(SupportsGet = true)]
        public int ArticleId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }

        public string Author { get; private set; }
        public Article Article { get; private set; }
        public ArticleContent ArticleContent { get; private set; }

        public int Views { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Article = await mService.QueryFullArticleByIdAsync(ArticleId);

            if (Article is not { Content: { Count: > 0 } })
            {
                return Redirect("/");
            }

            string locale = User.FindFirst(CustomClaimTypes.Language)?.Value ?? mLanguage.CurrentLocale;
            ArticleContent = Article.Content.FirstOrDefault(x => x.Locale.Equals(locale, StringComparison.InvariantCultureIgnoreCase))
                             ?? Article.Content.FirstOrDefault();

            if (ArticleContent?.Writer != null)
            {
                ClaimsPrincipal principal = await mSignInManager.CreateUserPrincipalAsync(ArticleContent.Writer);
                ProfileModel writer = await mUserManager.GetUserProfileAsync(ArticleContent.Writer, principal);
                if (writer != null)
                    Author = writer.Name;
            }

            ApplicationUser user = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                user = await mUserManager.GetUserAsync(User);
            }
            string readToken = Request.Cookies["ReadGuid"];
            ArticleReadToken readObj = await mService.QueryUserReadTokenAsync(ArticleId, readToken);
            string userAgent = Request.Headers["User-Agent"];

            if (readObj == null)
            {
                Article.Views += 1;
                await mService.SaveArticleAsync(Article);
                await mService.SaveReadUserTokenAsync(new ArticleReadToken
                {
                    ArticleId = (uint)ArticleId,
                    TokenId = readToken,
                    CreationDate = UnixTimestamp.Now(),
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0",
                    Referer = HttpContext.Request.Path,
                    UserAgent = userAgent,
                    UserId = user?.Id
                });
            }

            return Page();
        }
    }
}
