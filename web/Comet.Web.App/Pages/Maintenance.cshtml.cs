using Comet.Shared;
using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Model.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Comet.Web.App.Pages
{
    public class MaintenanceModel : PageModel
    {
        private readonly ApplicationDbContext mContext;

        public MaintenanceModel(ApplicationDbContext dbContext)
        {
            mContext = dbContext;
        }

        public bool HasTimer { get; set; }
        public DateTime MaintenanceEndTime { get; set; }
        public string Message { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!await HttpContext.IsUnderMaintenanceAsync(mContext))
            {
                return Redirect("/");
            }

            var maintenanceConfig = await mContext.SystemConfigs.FirstOrDefaultAsync(x => x.Key.Equals(SystemConfig.ConfigKey.MaintenanceSettings.ToString()));
            if (maintenanceConfig != null)
            {
                if (maintenanceConfig.Data2 != 0)
                {
                    MaintenanceEndTime = UnixTimestamp.ToDateTime((int) maintenanceConfig.Data2);
                    HasTimer = true;
                }

                if (!string.IsNullOrEmpty(maintenanceConfig.Text))
                    Message = maintenanceConfig.Text;
            }
            return Page();
        }
    }
}
