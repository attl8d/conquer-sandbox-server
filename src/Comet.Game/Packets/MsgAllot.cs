using Comet.Game.States;
using Comet.Network.Packets;
using Comet.Network.Packets.Game;
using System.Threading.Tasks;

namespace Comet.Game.Packets
{
    public sealed class MsgAllot : MsgAllot<Client>
    {
        /// <summary>
        ///     Process can be invoked by a packet after decode has been called to structure
        ///     packet fields and properties. For the server implementations, this is called
        ///     in the packet handler after the message has been dequeued from the server's
        ///     <see cref="PacketProcessor{TClient}" />.
        /// </summary>
        /// <param name="client">Client requesting packet processing</param>
        public override async Task ProcessAsync(Client client)
        {
            Character user = client.Character;

            if (Force > 0 && Force <= user.AttributePoints)
            {
                await user.AddAttributesAsync(ClientUpdateType.Strength, 1);
                await user.AddAttributesAsync(ClientUpdateType.Atributes, -1);
            }
            if (Speed > 0 && Speed <= user.AttributePoints)
            {
                await user.AddAttributesAsync(ClientUpdateType.Agility, 1);
                await user.AddAttributesAsync(ClientUpdateType.Atributes, -1);
            }
            if (Health > 0 && Health <= user.AttributePoints)
            {
                await user.AddAttributesAsync(ClientUpdateType.Vitality, 1);
                await user.AddAttributesAsync(ClientUpdateType.Atributes, -1);
            }
            if (Soul > 0 && Soul <= user.AttributePoints)
            {
                await user.AddAttributesAsync(ClientUpdateType.Spirit, 1);
                await user.AddAttributesAsync(ClientUpdateType.Atributes, -1);
            }
        }
    }
}
