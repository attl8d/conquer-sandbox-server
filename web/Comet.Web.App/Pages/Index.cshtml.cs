using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Services.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Web.App.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext mContext;
        private readonly UserManager mUserManager;

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context, UserManager userManager)
        {
            _logger = logger;
            mContext = context;
            mUserManager = userManager;
        }

        public List<Article> Articles { get; private set; } = new List<Article>();

        public async Task<IActionResult> OnGetAsync()
        {
            Articles = await mContext
                .Articles
                .AsQueryable()
                .Where(x => x.PublishDate <= DateTime.Now)
                .Include(x => x.Category)
                .Include(x => x.User)
                .Include(x => x.Content).ThenInclude(x => x.Writer)
                .Include(x => x.Content).ThenInclude(x => x.LastEditor)
                .OrderByDescending(x => x.Id)
                .Take(5)
                .ToListAsync();

            return Page();
        }
    }
}
