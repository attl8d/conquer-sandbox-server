using Comet.Game.States;
using Comet.Network.Packets;

namespace Comet.Game.Packets
{
    public sealed class MsgMailContent : MsgBase<Client>
    {
        public uint Data { get; set; }
        public string Content { get; set; }

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgMailContent);
            writer.Write(Data);
            writer.Write(Content);
            return writer.ToArray();
        }
    }
}
