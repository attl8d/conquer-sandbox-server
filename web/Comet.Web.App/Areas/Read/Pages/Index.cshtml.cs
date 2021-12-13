using Comet.Shared;
using Comet.Web.App.Areas.Profile.Models;
using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Services;
using Comet.Web.App.Services.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Web.App.Areas.Read.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext mContext;
        private readonly UserManager mUserManager;
        private readonly LanguageService mLanguage;

        public IndexModel(ApplicationDbContext context, UserManager userManager, LanguageService language)
        {
            mContext = context;
            mUserManager = userManager;
            mLanguage = language;
        }

        [BindProperty(SupportsGet = true)]
        public int ArticleId { get; set; }
        public string Author { get; private set; }
        public Article Article { get; private set; }
        public ArticleContent ArticleContent { get; private set; }

        public int Views { get; private set; }

        public List<ArticleComment> Comments { get; } = new List<ArticleComment>();

        public string CurrentUserName { get; private set; }
        public bool IsAllowedToComment { get; private set; }

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

            ApplicationUser user = null;
            if (User.Identity.IsAuthenticated)
            {
                user = await mUserManager.GetUserAsync(User);
                CurrentUserName = user.UserName;
            }
            string readToken = Request.Cookies["ReadGuid"];
            var readObj = await mContext.ArticleReadTokens
                .FirstOrDefaultAsync(x => x.ArticleId == ArticleId && x.TokenId.Equals(readToken));
            string userAgent = Request.Headers["User-Agent"];
            if (readObj == null)
            {
                Article.Views += 1;
                mContext.Update(Article);
                mContext.Update(new ArticleReadToken
                {
                    ArticleId = (uint)article.Id,
                    TokenId = readToken,
                    CreationDate = UnixTimestamp.Now(),
                    IpAddress = HttpContext.Connection.RemoteIpAddress.ToString(),
                    Referer = HttpContext.Request.Path,
                    UserAgent = userAgent,
                    UserId = user?.Id
                });
                await mContext.SaveChangesAsync();
            }

            var comments = await mContext.ArticleComments
                .Include(x => x.User)
                .Include(x => x.Article)
                .Where(x => x.ArticleId == ArticleId)
                .OrderBy(x => x.Id)
                .ToListAsync();
            foreach (var comment in comments)
            {
                var profile = await mUserManager.GetUserProfileAsync(comment.User);

                Comments.Add(new ArticleComment
                {
                    Identity = comment.Id,
                    Date = comment.CreationDate,
                    HasLike = false,
                    HasDislike = false,
                    Dislikes = comment.Dislikes,
                    Likes = comment.Likes,
                    Message = comment.Message,
                    UserName = profile?.Name ?? comment.User?.UserName ?? mLanguage.GetString("Visitor"),
                    Avatar = profile.ProfileImage ?? ""
                });
            }

            IsAllowedToComment = User.Identity.IsAuthenticated && !((bool)HttpContext.Items["Banned"]);
            return Page();
        }

        public struct ArticleComment
        {
            public int Identity;
            public string UserName;
            public string Avatar;
            public string Message;
            public DateTime Date;
            public int Likes;
            public int Dislikes;

            public bool HasLike;
            public bool HasDislike;
        }
    }
}
