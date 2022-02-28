namespace Comet.Network.Packets.Game
{
    public abstract class MsgGuide<T> : MsgBase<T>
    {
        public enum Request
        {
            InviteApprentice = 1,
            RequestMentor = 2,
            LeaveMentor = 3,
            ExpellApprentice = 4,
            AcceptRequestApprentice = 8,
            AcceptRequestMentor = 9,
            DumpApprentice = 18,
            DumpMentor = 19
        }

        public Request Action;
        public uint Identity;
        public uint Param;
        public uint Param2;
        public bool Online;
        public string Name;

        public override void Decode(byte[] bytes)
        {
            PacketReader reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Action = (Request)reader.ReadUInt32();
            Identity = reader.ReadUInt32();
            Param = reader.ReadUInt32();
            Param2 = reader.ReadUInt32();
            Online = reader.ReadBoolean();
            Name = reader.ReadString(reader.ReadByte());
        }

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgGuide);
            writer.Write((uint)Action);
            writer.Write(Identity);
            writer.Write(Param);
            writer.Write(Param2);
            writer.Write(Online);
            writer.Write(Name);
            return writer.ToArray();
        }
    }
}
