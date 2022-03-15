using System.Collections.Generic;
using System.Threading.Tasks;
using Comet.Web.Data.Entities;
using Comet.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Comet.Web.Areas.Guides.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ArticleService mService;

        public IndexModel(ArticleService service)
        {
            mService = service;
        }

        public List<ArticleCategory> Categories { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            Categories = await mService.QueryActiveArticleCategoriesByTypeCompleteAsync(2);
            return Page();
        }
    }
}