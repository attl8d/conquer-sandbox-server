using Comet.Game.States;
using Comet.Network.Packets;

namespace Comet.Game.Packets
{
    public sealed class MsgVipFunctionValidNotify : MsgBase<Client>
    {
        public int Flags { get; set; }

        public override byte[] Encode()
        {
            PacketWriter writer = new ();
            writer.Write((ushort)PacketType.MsgVipFunctionValidNotify);
            writer.Write(Flags);
            return writer.ToArray();
        }
    }
}
