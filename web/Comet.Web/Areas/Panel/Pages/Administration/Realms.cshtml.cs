using System.Collections.Generic;
using System.Threading.Tasks;
using Comet.Web.Services.Realms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Comet.Web.Areas.Panel.Pages.Administration
{
    public class RealmsModel : PageModel
    {
        private RealmService mRealmService;

        public RealmsModel(RealmService realms)
        {
            mRealmService = realms;
        }

        public List<RealmService.RealmPublicListData> Realms { get; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            await mRealmService.LoadRealmsAsync();
            Realms.AddRange(mRealmService.GetRealmList());
            return Page();
        }
    }
}
