using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Comet.Web.App.Services.Security;

namespace Comet.Web.App.Areas.Panel.Pages.Admin
{
    [Authorize(Policy = Moderator)]
    public class RolesModel : PageModel
    {
        public void OnGet()
        {
            
        }
    }
}
