using Comet.Shared;
using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Comet.Web.App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Comet.Web.App.Model.Helpers
{
    public static class HttpContextHelper
    {
        public static async Task<bool> IsUnderMaintenanceAsync(this HttpContext context, ApplicationDbContext dbContext)
        {
            var maintenanceConfig = await dbContext.SystemConfigs.FirstOrDefaultAsync(x => x.Key.Equals(SystemConfig.ConfigKey.MaintenanceSettings.ToString()));
            if (maintenanceConfig != null)
            {
                if (maintenanceConfig.Data0 != 0) // maintenance is active
                {
                    int now = UnixTimestamp.Now();
                    bool currentlyDisabled = maintenanceConfig.Data1 == 0 && maintenanceConfig.Data2 == 0;
                    bool timeCheckDisable = now >= maintenanceConfig.Data1 && now <= maintenanceConfig.Data2;
                    bool userIsAllowed = context.User.Identity.IsAuthenticated
                        && context.User.IsInRole(Security.Roles.Administrator);

                    return currentlyDisabled || timeCheckDisable;
                }
            }

            return false;
        }
    }
}
