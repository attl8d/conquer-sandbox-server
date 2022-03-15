#define VIP_MODE_CONTRIBUTION
// #define VIP_MODE_CARD
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Comet.Web.Areas.Panel.Pages.User
{
    [Authorize]
    public class VIPModel : PageModel
    {
#if VIP_MODE_CONTRIBUTION
        public const VipSystemMode Mode = VipSystemMode.Contribution;
#else
        public const VipSystemMode Mode = VipSystemMode.Card;
#endif

        public List<VipCardStruct> Vips { get; private set; } = new();
        public List<ServerInfo> Servers { get; private set; } = new();
        public List<VipCardUsageStruct> VipUsage { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }

        public struct VipCardStruct
        {
            public uint Identity { get; set; }
            public string Name { get; set; }
            public int VipLevel { get; set; }
            public int VipDays { get; set; }
        }

        public struct VipCardUsageStruct
        {
            public uint Identity { get; set; }
            public int VipLevel { get; set; }
            public int VipDays { get; set; }
            public string ServerName { get; set; }
            public string CharacterName { get; set; }
            public string BindDate { get; set; }
        }

        public struct ServerInfo
        {
            public int Identity { get; set; }
            public string Name { get; set; }
        }

        public enum VipSystemMode
        {
            Contribution,
            Card
        }
    }
}