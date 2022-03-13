using System.Collections.Generic;
using System.Threading.Tasks;
using Comet.Web.Data.Entities;
using Comet.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Comet.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ArticleService mArticleService;
        private readonly ILogger<IndexModel> mLogger;

        public IndexModel(ILogger<IndexModel> logger, ArticleService articleRepo)
        {
            mLogger = logger;
            mArticleService = articleRepo;
        }

        public List<Article> Articles { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            Articles = await mArticleService.GetAsync();
            return Page();
        }
    }
}