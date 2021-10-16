using Comet.Game.States;
using Comet.Network.Packets;
using System.Threading.Tasks;

namespace Comet.Game.Packets
{
    public sealed class MsgHangUp : MsgBase<Client>
    {
        public HangUpMode Action { get; set; }
        public ushort Param { get; set; }
        public ushort Param1 { get; set; }
        public ushort Param2 { get; set; }
        public ushort Param3 { get; set; }
        public ulong Experience { get; set; }

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort) PacketType.MsgHangUp);
            writer.Write((ushort)Action);
            writer.Write(Param);
            writer.Write(Param1);
            writer.Write(Param2);
            writer.Write(Experience);
            return writer.ToArray();
        }

        public override void Decode(byte[] bytes)
        {
            PacketReader reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Action = (HangUpMode)reader.ReadUInt32();
            Param = reader.ReadUInt16();
            Param1 = reader.ReadUInt16();
            Param2 = reader.ReadUInt16();
            Param3 = reader.ReadUInt16();
            Experience = reader.ReadUInt64();
        }

        public override Task ProcessAsync(Client client)
        {
            return base.ProcessAsync(client);
        }

        public enum HangUpMode
        {
            Show,
            Start,
            Interface,
            End
        }
    }
}
