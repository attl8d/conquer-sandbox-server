using Quartz;
using System;
using System.Threading.Tasks;
using static Comet.Web.App.Data.Entities.LogHistory;

namespace Comet.Web.App.Services.Schedule
{
    [DisallowConcurrentExecution]
    public class DailyGameSynchronizationJob : IJob
    {
        private readonly LogService<DailyGameSynchronizationJob> mLog;

        public DailyGameSynchronizationJob(LogService<DailyGameSynchronizationJob> log)
        {
            mLog = log;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await mLog.LogAsync($"Daily Update Triggered! {DateTime.Now:O}", LogType.None);
        }
    }
}
