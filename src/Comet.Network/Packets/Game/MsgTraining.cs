namespace Comet.Network.Packets.Game
{
    public abstract class MsgTraining<T> : MsgBase<T>
    {
        public enum Mode
        {
            RequestTime,
            RequestEnter,
            Unknown2,
            RequestRewardInfo,
            ClaimReward
        }

        public Mode Action { get; set; }
        public ulong TrainingTime { get; set; }

        public override void Decode(byte[] bytes)
        {
            PacketReader reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Action = (Mode)reader.ReadUInt32();
            TrainingTime = reader.ReadUInt64();
        }

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgTraining);
            writer.Write((uint)Action);
            writer.Write(TrainingTime);
            return writer.ToArray();
        }
    }
}
