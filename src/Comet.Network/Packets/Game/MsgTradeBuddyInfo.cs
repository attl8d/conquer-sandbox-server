namespace Comet.Network.Packets.Game
{
    public abstract class MsgTradeBuddyInfo<T> : MsgBase<T>
    {
        public uint Identity;
        public uint Lookface;
        public byte Level;
        public byte Profession;
        public ushort PkPoints;
        public uint Syndicate;
        public int SyndicatePosition;
        public ushort Unknown;
        public string Name;

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgTradeBuffyInfo);
            writer.Write(Identity);
            writer.Write(Lookface);
            writer.Write(Level);
            writer.Write(Profession);
            writer.Write(PkPoints);
            writer.Write(Syndicate);
            writer.Write(SyndicatePosition);
            writer.Write(Unknown);
            writer.Write(Name, 16);
            return writer.ToArray();
        }
    }
}
