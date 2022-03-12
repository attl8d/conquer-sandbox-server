using Comet.AI.World.Managers;
using Comet.Shared;
using System.Threading.Tasks;

namespace Comet.AI.World.Threads
{
    public class AiThread : TimerBase
    {
        public AiThread() 
            : base(300, "Ai Thread")
        {
        }

        protected override async Task<bool> OnElapseAsync()
        {
            await RoleManager.OnTimerAsync();
            return true;
        }
    }
}
