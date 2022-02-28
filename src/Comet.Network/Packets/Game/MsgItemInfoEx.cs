namespace Comet.Network.Packets.Game
{
    public abstract class MsgItemInfoEx<T> : MsgBase<T>
    {
        public enum ViewMode : ushort
        {
            None,
            Silvers,
            Unknown,
            Emoney,
            ViewEquipment
        }

        public uint Identity;
        public uint TargetIdentity;
        public uint Price;
        public uint ItemType;
        public ushort Amount;
        public ushort AmountLimit;
        public ViewMode Mode;
        public ushort Position;
        public uint SocketProgress;
        public byte SocketOne;
        public byte SocketTwo;
        public byte Addition;
        public byte Blessing;
        public byte Enchantment;
        public byte Color;
        public uint CompositionProgress;

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgItemInfoEx);
            writer.Write(Identity);
            writer.Write(TargetIdentity);
            writer.Write(Price);
            writer.Write(ItemType);
            writer.Write(Amount);
            writer.Write(AmountLimit);
            writer.Write((ushort)Mode);
            writer.Write(Position);
            writer.Write(SocketProgress);
            writer.Write(SocketOne);
            writer.Write(SocketTwo);
            writer.Write((ushort)0);
            writer.Write(Addition);
            writer.Write(Blessing);
            writer.Write(Enchantment);
            writer.Write((byte)0);
            writer.Write(0UL);
            writer.Write((ushort)Color);
            writer.Write(CompositionProgress);
            return writer.ToArray();
        }
    }
}
