using Comet.Game.Internal.AI;
using Comet.Network.Packets.Ai;
using System.Threading.Tasks;

namespace Comet.Game.Packets.Ai
{
    public sealed class MsgAiPing : MsgAiPing<AiClient>
    {
        public override Task ProcessAsync(AiClient client)
        {
            // Keep alive
            return Task.CompletedTask;
        }
    }
}
