using Comet.AI.States;
using Comet.Network.Packets.Game;
using System.Drawing;

namespace Comet.AI.Packets
{
    public sealed class MsgTalk : MsgTalk<Server>
    {
        public MsgTalk(uint characterID, TalkChannel channel, Color color, string recipient, string sender, string text) 
            : base(characterID, channel, color, recipient, sender, text)
        {
        }
    }
}
