using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Comet.Web.Services.Security;

namespace Comet.Web.Areas.Panel.Pages.Administration
{
    [Authorize(Policy = Moderator)]
    public class UsersModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
