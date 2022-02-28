using Comet.AI.States;
using Comet.AI.World.Managers;
using Comet.Network.Packets.Game;
using System.Threading.Tasks;

namespace Comet.AI.Packets
{
    public sealed class MsgWalk : MsgWalk<Server>
    {
        public override async Task ProcessAsync(Server client)
        {
            Role target = RoleManager.GetRole(Identity);
            if (target == null)
                return;

            await target.MoveTowardAsync(Direction, Mode, false);
        }
    }
}
