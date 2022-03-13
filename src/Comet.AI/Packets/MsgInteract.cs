using System.Threading.Tasks;
using Comet.AI.States;
using Comet.AI.World.Managers;
using Comet.Network.Packets.Game;

namespace Comet.AI.Packets
{
    public sealed class MsgInteract : MsgInteract<Server>
    {
        public override async Task ProcessAsync(Server client)
        {
            Role sender = RoleManager.GetRole(SenderIdentity);
            if (sender == null)
                return;

            Role target = sender?.Map.QueryAroundRole(sender, TargetIdentity);
            if (target == null && Action != MsgInteractType.MagicAttack)
                return;

            if (!target.IsAlive)
                return;

            switch (Action)
            {
                case MsgInteractType.Attack:
                case MsgInteractType.Shoot:
                case MsgInteractType.MagicAttack:
                {
                    await target.BeAttackAsync(sender);
                    break;
                }
            }
        }
    }
}