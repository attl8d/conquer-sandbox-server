using Comet.Game.Database.Models;
using Comet.Game.States;
using Comet.Shared;
using Quartz;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comet.Game.World.Schedule
{
    [DisallowConcurrentExecution]

    public sealed class AutomaticActionJob : IJob
    {
        private const int _ACTION_SYSTEM_EVENT = 2030000;
        private const int _ACTION_SYSTEM_EVENT_LIMIT = 9999;

        private readonly ConcurrentDictionary<uint, DbAction> m_dicActions;

        public AutomaticActionJob()
        {
            m_dicActions = new ConcurrentDictionary<uint, DbAction>(1, _ACTION_SYSTEM_EVENT_LIMIT);

            for (int a = 0; a < _ACTION_SYSTEM_EVENT_LIMIT; a++)
            {
                DbAction action = Kernel.EventManager.GetAction((uint)(_ACTION_SYSTEM_EVENT + a));
                if (action != null)
                    m_dicActions.TryAdd(action.Identity, action);
            }
        }

        public async Task Execute(IJobExecutionContext context)
        {
            foreach (var action in m_dicActions.Values)
            {
                try
                {
                    await GameAction.ExecuteActionAsync(action.Identity, null, null, null, "");
                }
                catch (Exception ex)
                {
                    await Log.WriteLogAsync(LogLevel.Exception, ex.ToString());
                }
            }
        }
    }
}
