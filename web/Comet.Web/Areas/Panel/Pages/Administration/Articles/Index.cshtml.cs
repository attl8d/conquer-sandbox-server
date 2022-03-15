using System.Collections.Generic;
using System.Threading.Tasks;
using Comet.Web.Model.Articles;
using Comet.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Comet.Web.Areas.Panel.Pages.Administration.Articles
{
    public class IndexModel : PageModel
    {
        private readonly ArticleService mService;

        public IndexModel(ArticleService service)
        {
            mService = service;
        }

        public List<ArticleCategoryTypeModel> ArticleCategoriesTypes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            ArticleCategoriesTypes = await mService.QueryActiveArticleCategoryTypeAsync();
            return Page();
        }
    }
}