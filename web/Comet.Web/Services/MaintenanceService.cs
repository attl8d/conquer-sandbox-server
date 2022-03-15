using System;
using System.Threading.Tasks;
using Comet.Shared;
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

        public async Task<bool> IsUnderMaintenanceAsync()
        {
            SystemConfig config = await GetAsync();
            if (config == null) return false;
            if (config.Data0 != 0) return true;

            DateTime start = DateTime.MaxValue;
            DateTime end = DateTime.MinValue;
            DateTime now = DateTime.Now;

            if (config.Data1 != 0)
                start = UnixTimestamp.ToDateTime((uint) config.Data1);
            if (config.Data2 != 0)
                end = UnixTimestamp.ToDateTime((uint)config.Data2);

            if (config.Data1 != 0 && config.Data2 != 0)
                return now >= start && now <= end;
            if (config.Data1 != 0)
                return now >= start;
            return now <= end;

        }

        public async Task<bool> SaveAsync(SystemConfig config)
        {
            if (config.Data1 > config.Data2)
                return false;
            if (config.Data2 < config.Data1)
                return false;
            if (string.IsNullOrEmpty(config.Text))
                return false;
            config.Data0 = Math.Min(1, Math.Max(0, config.Data0));
            return await mContext.SaveAsync(config);
        }
    }
}