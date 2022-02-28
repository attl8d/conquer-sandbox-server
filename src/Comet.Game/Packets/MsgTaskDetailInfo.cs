using Comet.Game.States;
using Comet.Network.Packets.Game;
using System.Threading.Tasks;

namespace Comet.Game.Packets
{
    public sealed class MsgTaskDetailInfo : MsgTaskDetailInfo<Client>
    {
        public override async Task ProcessAsync(Client client)
        {
            
        }
    }
}