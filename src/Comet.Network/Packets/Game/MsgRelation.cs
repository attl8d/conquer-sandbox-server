namespace Comet.Network.Packets.Game
{
    public abstract class MsgRelation<T> : MsgBase<T>
    {
        public uint SenderIdentity;
        public uint TargetIdentity;
        public int Level;
        public int BattlePower;
        public bool IsSpouse;
        public bool IsTutor;
        public bool IsTradePartner;

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgRelation);
            writer.Write(SenderIdentity);
            writer.Write(TargetIdentity);
            writer.Write(Level);
            writer.Write(BattlePower);
            writer.Write(IsSpouse ? 1 : 0);
            writer.Write(IsTutor ? 1 : 0);
            writer.Write(IsTradePartner ? 1 : 0);
            return writer.ToArray();
        }
    }
}