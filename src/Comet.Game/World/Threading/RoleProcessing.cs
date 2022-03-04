using Comet.Game.World.Managers;
using Comet.Shared.Comet.Shared;
using System.Threading.Tasks;

namespace Comet.Game.World.Threading
{
    public class RoleProcessing : TimerBase
    {
        public RoleProcessing()
            : base(200, "Ai Thread")
        {
        }

        protected override async Task<bool> OnElapseAsync()
        {
            //await RoleManager.OnRoleTimerAsync();
            await MapManager.OnTimerAsync();
            return true;
        }
    }
}
