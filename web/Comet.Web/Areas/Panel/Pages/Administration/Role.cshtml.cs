using Comet.Web.Data.Entities;
using Comet.Web.Services;
using Comet.Web.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Comet.Web.Services.Security;

namespace Comet.Web.Areas.Panel.Pages.Administration
{
    [Authorize(Policy = Moderator)]
    public class RoleModel : PageModel
    {
        private readonly RoleManager mRoleManager;

        public RoleModel(RoleManager roleManager)
        {
            mRoleManager = roleManager;
        }

        public Dictionary<string, Claims> Claims = new Dictionary<string, Claims>();

        [BindProperty(SupportsGet = true)] public int RoleId { get; set; }

        public ApplicationRole Role { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Role = await mRoleManager.FindByIdAsync(RoleId.ToString());

            Claims = new Dictionary<string, Claims>(Enum.GetValues<Claims>().ToDictionary(x => x.ToString()));

            if (Role == null)
                return NotFound();

            return Page();
        }
    }
}
