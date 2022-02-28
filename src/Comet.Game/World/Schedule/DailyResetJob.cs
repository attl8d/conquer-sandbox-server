using Comet.Game.World.Managers;
using Quartz;
using System.Threading.Tasks;

namespace Comet.Game.World.Schedule
{
    [DisallowConcurrentExecution]
    public sealed class DailyResetJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await RoleManager.OnDailyTriggerAsync();   
        }
    }
}
