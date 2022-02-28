using Comet.AI.States;
using Comet.Network.Packets.Ai;
using System.Threading.Tasks;

namespace Comet.AI.Packets
{
    public sealed class MsgAiPing : MsgAiPing<Server>
    {
        public override Task ProcessAsync(Server client)
        {
            // Keep alive
            return Task.CompletedTask;
        }
    }
}
