using Comet.Game.World.Managers;
using Comet.Shared.Comet.Shared;
using System.Threading.Tasks;

namespace Comet.Game.World.Threading
{
    public class UserProcessing : TimerBase
    {
        public UserProcessing()
            : base(60, "User Thread")
        {
        }

        protected override async Task<bool> OnElapseAsync()
        {
            await RoleManager.OnTimerAsync();
            return true;
        }
    }
}