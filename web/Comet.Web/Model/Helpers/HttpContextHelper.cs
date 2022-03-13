using System.Threading.Tasks;
using Comet.Shared;
using Comet.Web.Data;
using Comet.Web.Data.Entities;
using Comet.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Comet.Web.Model.Helpers
{
    public static class HttpContextHelper
    {
        public static async Task<bool> IsUnderMaintenanceAsync(this HttpContext context, ApplicationDbContext dbContext)
        {
            SystemConfig maintenanceConfig =
                await dbContext.SystemConfigs.FirstOrDefaultAsync(
                    x => x.Key.Equals(SystemConfig.ConfigKey.MaintenanceSettings.ToString()));
            if (maintenanceConfig != null)
                if (maintenanceConfig.Data0 != 0) // maintenance is active
                {
                    int now = UnixTimestamp.Now();
                    bool currentlyDisabled = maintenanceConfig.Data1 == 0 && maintenanceConfig.Data2 == 0;
                    bool timeCheckDisable = now >= maintenanceConfig.Data1 && now <= maintenanceConfig.Data2;
                    bool userIsAllowed = context.User.Identity.IsAuthenticated
                                         && context.User.IsInRole(Security.Administrator);

                    return currentlyDisabled || timeCheckDisable;
                }

            return false;
        }
    }
}