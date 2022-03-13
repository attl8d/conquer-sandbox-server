using System;
using System.Threading.Tasks;
using Comet.Shared;
using Comet.Web.Data;
using Comet.Web.Data.Entities;
using Comet.Web.Model.Helpers;
using Comet.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Comet.Web.Pages
{
    public class MaintenanceModel : PageModel
    {
        private readonly ApplicationDbContext mContext;
        private readonly MaintenanceService mService;

        public MaintenanceModel(ApplicationDbContext dbContext, MaintenanceService repository)
        {
            mContext = dbContext;
            mService = repository;
        }

        public bool HasTimer { get; set; }
        public DateTime MaintenanceEndTime { get; set; }
        public string Message { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!await HttpContext.IsUnderMaintenanceAsync(mContext)) return Redirect("/");

            SystemConfig maintenanceConfig = await mService.GetAsync();
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