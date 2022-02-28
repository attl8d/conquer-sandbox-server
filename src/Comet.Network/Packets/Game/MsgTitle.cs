using System.Collections.Generic;

namespace Comet.Network.Packets.Game
{
    public abstract class MsgTitle<T> : MsgBase<T>
    {
        public uint Identity;
        public byte Title;
        public TitleAction Action;
        public byte Count;
        public List<byte> Titles = new List<byte>();

        public override void Decode(byte[] bytes)
        {
            PacketReader reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Identity = reader.ReadUInt32();
            Title = reader.ReadByte();
            Action = (TitleAction)reader.ReadByte();
            Count = reader.ReadByte();
            for (int i = 0; i < Count; i++)
                Titles.Add(reader.ReadByte());
        }

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgTitle);
            writer.Write(Identity); // 4
            writer.Write(Title);
            writer.Write((byte)Action);
            writer.Write(Count = (byte)Titles.Count);
            foreach (var b in Titles)
                writer.Write(b);
            return writer.ToArray();
        }

        public enum TitleAction : byte
        {
            Hide = 0,
            Add = 1,
            Remove = 2,
            Select = 3,
            Query = 4
        }
    }
}
