using Comet.Game.States;
using Comet.Network.Packets;
using System.Collections.Generic;
using static Comet.Game.States.Fate;

namespace Comet.Game.Packets
{
    public sealed class MsgTrainingVitalityInfo : MsgBase<Client>
    {
        /// <remarks>
        /// 1 update 0 normal
        /// </remarks>
        public ushort Mode { get; set; } 
        public uint Identity { get; set; }
        public uint Strength { get; set; }
        public uint Data { get; set; }
        public int Count { get; set; }
        public List<TrainingData> Datas { get; set; } = new List<TrainingData>();

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort) PacketType.MsgTrainingVitalityInfo);
            writer.Write(Mode);
            writer.Write(Identity);
            writer.Write(Strength);
            writer.Write(Data);
            writer.Write(Count = Datas.Count);
            foreach (var data in Datas)
            {
                writer.Write((byte)data.Type);
                writer.Write(data.Power1);
                writer.Write(data.Power2);
                writer.Write(data.Power3);
                writer.Write(data.Power4);
            }
            return writer.ToArray();
        }

        public struct TrainingData
        {
            public FateType Type;
            public int Power1;
            public int Power2;
            public int Power3;
            public int Power4;
        }
    }
}
