using System.Threading.Tasks;
using Comet.AI.States;
using Comet.AI.World.Managers;
using Comet.Game.Packets;

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
                    var newX = (ushort) Command;
                    var newY = (ushort) (Command >> 16);
                    target.QueueAction(() => target.JumpPosAsync(newX, newY, false));
                    break;
                }
            }
        }
    }
}