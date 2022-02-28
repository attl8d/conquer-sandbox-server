using Comet.Game.World.Managers;
using Comet.Shared.Comet.Shared;
using System.Threading.Tasks;

namespace Comet.Game.World.Threading
{
    public class AiProcessing : TimerBase
    {
        public AiProcessing()
            : base(200, "Ai Thread")
        {
        }

        protected override async Task<bool> OnElapseAsync()
        {
            await MapManager.OnTimerAsync();
            return true;
        }
    }
}
