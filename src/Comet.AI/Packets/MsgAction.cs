using Comet.AI.States;
using Comet.AI.World.Managers;
using Comet.Game.Packets;
using System.Threading.Tasks;

namespace Comet.AI.Packets
{
    public sealed class MsgAction : MsgAction<Server>
    {
        public override async Task ProcessAsync(Server client)
        {
            Role target = RoleManager.GetRole(Identity);

            switch (Action)
            {
                case ActionType.MapJump:
                    {
                        ushort newX = (ushort)Command;
                        ushort newY = (ushort)(Command >> 16);
                        target.QueueAction(() => target.JumpPosAsync(newX, newY, false));
                        break;
                    }
            }
        }
    }
}
