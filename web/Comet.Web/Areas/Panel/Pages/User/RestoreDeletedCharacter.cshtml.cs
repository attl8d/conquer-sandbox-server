using System.Collections.Generic;
using System.Threading.Tasks;
using Comet.Web.Services.Realms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Comet.Web.Areas.Panel.Pages.User
{
    [Authorize]
    public class RestoreDeletedCharacterModel : PageModel
    {
        private readonly RealmService mRealmService;

        public RestoreDeletedCharacterModel(RealmService realmService)
        {
            mRealmService = realmService;
        }

        public List<RealmService.RealmPublicListData> Realms { get; private set; } = new List<RealmService.RealmPublicListData>();

        public async Task<IActionResult> OnGetAsync()
        {
            await mRealmService.LoadRealmsAsync();

            Realms.AddRange(mRealmService.GetRealmList());

            return Page();
        }
    }
}
