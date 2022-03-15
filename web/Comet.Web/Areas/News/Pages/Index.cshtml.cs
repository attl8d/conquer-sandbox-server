using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Comet.Web.Data.Entities;
using Comet.Web.Model.Articles;
using Comet.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Comet.Web.Areas.News.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ArticleService mService;

        public IndexModel(ArticleService service)
        {
            mService = service;
        }

        public int PageCount { get; set; }

        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; }

        [BindProperty(SupportsGet = true)] public string CategoryName { get; set; }

        public List<Article> Articles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            const int ipp = 10;
            int page = Math.Max(0, CurrentPage);
            bool all = true;

            if (!string.IsNullOrEmpty(CategoryName))
            {
                List<ArticleCategoryModel> allCategories = await mService.QueryActiveArticleCategoriesByTypeAsync(1);
                ArticleCategoryModel category = allCategories.FirstOrDefault(x => x.RealName.ToLower().Equals(CategoryName.ToLower()));
                if (category != null)
                {
                    Articles = await mService.GetByCategoryAsync(category.Id, CurrentPage, ipp);
                    all = false;
                }
            }

            if (all)
            {
                Articles = await mService.GetAsync(1, page, ipp);
            }
            return Page();
        }
    }
}
