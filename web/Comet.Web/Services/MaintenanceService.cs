using Comet.Web.Data;
using Comet.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Comet.Web.Services
{
    public class MaintenanceService
    {
        private readonly ApplicationDbContext mContext;

        public MaintenanceService(ApplicationDbContext context)
        {
            mContext = context;
        }

        public Task<SystemConfig> GetAsync() => mContext.SystemConfigs.FirstOrDefaultAsync(x => x.Key.Equals(SystemConfig.ConfigKey.MaintenanceSettings.ToString()));
    }
}
