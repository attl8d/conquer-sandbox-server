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

        public List<ArticleCategoriesType> ArticleCategoriesTypes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            List<ArticleCategoryTypeModel> articleCategoriesTypes =
                await mService.QueryActiveArticleCategoryTypeAsync();
            foreach (ArticleCategoryTypeModel type in articleCategoriesTypes)
            {
                var cat = new ArticleCategoriesType
                {
                    Id = type.Id,
                    Name = type.Name,
                    Categories = await mService.QueryActiveArticleCategoriesByTypeAsync(type.Id)
                };
                ArticleCategoriesTypes.Add(cat);
            }

            return Page();
        }

        public struct ArticleCategoriesType
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<ArticleCategoryModel> Categories { get; set; }
        }
    }
}