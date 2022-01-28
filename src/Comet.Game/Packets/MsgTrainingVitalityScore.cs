using Comet.Game.States;
using Comet.Network.Packets;
using static Comet.Game.States.Fate;

namespace Comet.Game.Packets
{
    public sealed class MsgTrainingVitalityScore : MsgBase<Client>
    {
        public FateType AttrType { get; set; }
        public int Power { get; set; }
        public string Name { get; set; }

        public override byte[] Encode()
        {
            PacketWriter writer = new();
            writer.Write((ushort)PacketType.MsgTrainingVitalityScore);
            writer.Write((byte)AttrType);
            writer.Write(Power);
            writer.Write(Name, 16);
            return writer.ToArray();
        }
    }
}
