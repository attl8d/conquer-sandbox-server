namespace Comet.Network.Packets.Game
{
    public abstract class MsgGuideContribute<T> : MsgBase<T>
    {
        public enum RequestType
        {
            ClaimExperience = 1,
            ClaimHeavenBlessing = 2,
            ClaimItemAdd = 3,
            Query = 4
        }

        public RequestType Mode;
        public uint Identity;
        public byte[] Padding = new byte[12];
        public uint Experience;
        public ushort HeavenBlessing;
        public ushort Composing;
        public ushort Test1;
        public ushort Test2;

        public override void Decode(byte[] bytes)
        {
            PacketReader reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Mode = (RequestType)reader.ReadUInt32();
            Identity = reader.ReadUInt32();
            Padding = reader.ReadBytes(12);
            Experience = reader.ReadUInt32();
            HeavenBlessing = reader.ReadUInt16();
            Composing = reader.ReadUInt16();
            Test1 = reader.ReadUInt16();
            Test2 = reader.ReadUInt16();
        }

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgGuideContribute);
            writer.Write((int)Mode);
            writer.Write(Identity);
            writer.Write(Padding);
            writer.Write(Experience);
            writer.Write(Test1);
            writer.Write(Test2);
            writer.Write(HeavenBlessing);
            writer.Write(Composing);
            return writer.ToArray();
        }
    }
}
