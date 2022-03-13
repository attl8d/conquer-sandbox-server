using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Comet.Web.Data.Entities;
using Comet.Web.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Comet.Web.Services.Security;

namespace Comet.Web.Areas.Panel.Pages.Administration
{
    [Authorize(Policy = Moderator)]
    public class RoleModel : PageModel
    {
        private readonly RoleManager mRoleManager;

        public Dictionary<string, Claims> Claims = new();

        public RoleModel(RoleManager roleManager)
        {
            mRoleManager = roleManager;
        }

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