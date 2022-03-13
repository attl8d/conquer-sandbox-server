using Comet.Game.States;
using Comet.Game.World.Managers;
using Comet.Network.Packets.Game;
using System.Threading.Tasks;

namespace Comet.Game.Packets
{
    public sealed class MsgWalk : MsgWalk<Client>
    {
        /// <summary>
        ///     Process can be invoked by a packet after decode has been called to structure
        ///     packet fields and properties. For the server implementations, this is called
        ///     in the packet handler after the message has been dequeued from the server's
        ///     <see cref="PacketProcessor" />.
        /// </summary>
        /// <param name="client">Client requesting packet processing</param>
        public override async Task ProcessAsync(Client client)
        {
            if (client != null)
            {
                await client.Character.ProcessOnMoveAsync();

                if (await client.Character.MoveTowardAsync(Direction, Mode))
                {
                    await client.SendAsync(this);
                    await client.Character.Screen.UpdateAsync(this);
                    await Kernel.BroadcastWorldMsgAsync(this);
                }
                return;
            }

            Role target = RoleManager.GetRole(Identity);
            if (target == null)
                return;

            await target.ProcessOnMoveAsync();
            await target.MoveTowardAsync(Direction, Mode);
            await target.BroadcastRoomMsgAsync(this, false);
            await Kernel.BroadcastWorldMsgAsync(this);
        }
    }
}
