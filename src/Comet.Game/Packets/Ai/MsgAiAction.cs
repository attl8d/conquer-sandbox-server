using System.Threading.Tasks;
using Comet.Game.Internal.AI;
using Comet.Network.Packets.Ai;

namespace Comet.Game.Packets.Ai
{
    public sealed class MsgAiAction : MsgAiAction<AiClient>
    {
        public override async Task ProcessAsync(AiClient client)
        {
            switch (Action)
            {
            }
        }
    }
}