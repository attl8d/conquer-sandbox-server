using Comet.Game.States;
using Comet.Network.Packets.Game;
using System.Threading.Tasks;

namespace Comet.Game.Packets
{
    public sealed class MsgNpcInfo : MsgNpcInfo<Client>
    {
        public override Task ProcessAsync(Client client)
        {
            return GameAction.ExecuteActionAsync(client.Character.InteractingItem, client.Character, null, null,
                $"{PosX} {PosY} {Lookface} {Identity} {NpcType}");
        }
    }

}
