using Comet.Web.App.Services.Realms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Comet.Web.App.Services.Security;

namespace Comet.Web.App.Areas.Panel.Pages.Admin
{
    [Authorize(Policy = SuperAdministrator)]
    public class RealmsModel : PageModel
    {
        private RealmService mRealmService;

        public RealmsModel(RealmService realms)
        {
            mRealmService = realms;
        }

        public List<RealmService.RealmPublicListData> Realms { get; } = new List<RealmService.RealmPublicListData>();

        public async Task<IActionResult> OnGetAsync()
        {
            await mRealmService.LoadRealmsAsync();
            Realms.AddRange(mRealmService.GetRealmList());
            return Page();
        }
    }
}
