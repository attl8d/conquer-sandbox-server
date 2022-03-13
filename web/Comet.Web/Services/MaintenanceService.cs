using System.Threading.Tasks;
using Comet.Web.Data;
using Comet.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Comet.Web.Services
{
    public class MaintenanceService
    {
        private readonly ApplicationDbContext mContext;

        public MaintenanceService(ApplicationDbContext context)
        {
            mContext = context;
        }

        public Task<SystemConfig> GetAsync()
        {
            return mContext.SystemConfigs.FirstOrDefaultAsync(
                x => x.Key.Equals(SystemConfig.ConfigKey.MaintenanceSettings.ToString()));
        }
    }
}