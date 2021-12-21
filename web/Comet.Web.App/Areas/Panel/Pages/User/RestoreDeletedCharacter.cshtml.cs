using Comet.Web.App.Services.Realms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Comet.Web.App.Services.Realms.RealmService;

namespace Comet.Web.App.Areas.Panel.Pages.User
{
    [Authorize]
    public class RestoreDeletedCharacterModel : PageModel
    {
        private readonly RealmService mRealmService;

        public RestoreDeletedCharacterModel(RealmService realmService)
        {
            mRealmService = realmService;
        }

        public List<RealmPublicListData> Realms { get; private set; } = new List<RealmPublicListData>();

        public async Task<IActionResult> OnGetAsync()
        {
            await mRealmService.LoadRealmsAsync();

            Realms.AddRange(mRealmService.GetRealmList());

            return Page();
        }
    }
}
