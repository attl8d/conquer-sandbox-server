using Comet.Network.Packets;
using System.Collections.Generic;

namespace Comet.Network.Packets.Game
{
    public abstract class MsgPigeon<T> : MsgBase<T>
    {
        public enum PigeonMode
        {
            None,
            Query,
            QueryUser,
            Send,
            SuperUrgent,
            Urgent
        }

        public PigeonMode Mode { get; set; }
        public int Param { get; set; }
        public List<string> Strings = new List<string>();

        public override void Decode(byte[] bytes)
        {
            PacketReader reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Mode = (PigeonMode)reader.ReadInt32();
            Param = reader.ReadInt32();
            Strings = reader.ReadStrings();
        }

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgPigeon);
            writer.Write((int)Mode);
            writer.Write(Param);
            writer.Write(Strings);
            return writer.ToArray();
        }
    }
}