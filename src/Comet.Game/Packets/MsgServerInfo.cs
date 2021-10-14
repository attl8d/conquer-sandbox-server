using Comet.Game.States;
using Comet.Network.Packets;

namespace Comet.Game.Packets
{
    public sealed class MsgServerInfo : MsgBase<Client>
    {
        public int ClassicMode { get; set; }
        public int PotencyMode { get; set; }

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgServerInfo);
            writer.Write(ClassicMode);
            writer.Write(PotencyMode);
            return writer.ToArray();
        }
    }
}
