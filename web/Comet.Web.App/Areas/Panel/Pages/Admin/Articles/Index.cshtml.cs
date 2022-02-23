using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Web.App.Areas.Panel.Pages.Admin.Articles
{
    public class IndexModel : PageModel
    {
        private ApplicationDbContext mContext { get; set; }

        public IndexModel(ApplicationDbContext context)
        {
            mContext = context;
        }

        public List<ArticleCategoryType> ArticleCategorieTypes { get; set; } = new List<ArticleCategoryType>();

        public async Task<IActionResult> OnGetAsync()
        {
            ArticleCategorieTypes = await mContext.ArticleCategoryTypes.AsQueryable().Where(x => x.Flags == 0).ToListAsync();
            return Page();
        }
    }
}
