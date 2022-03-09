using Comet.Web.Data.Entities;
using Comet.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comet.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> mLogger;
        private readonly ArticleService mArticleService;

        public IndexModel(ILogger<IndexModel> logger, ArticleService articleRepo)
        {
            mLogger = logger;
            mArticleService = articleRepo;
        }

        public List<Article> Articles { get; private set; } = new List<Article>();

        public async Task<IActionResult> OnGetAsync()
        {
            Articles = await mArticleService.GetAsync(0, 5);
            return Page();
        }
    }
}