using System.Collections.Generic;

namespace Comet.Network.Packets.Game
{
    public abstract class MsgTotemPoleInfo<T> : MsgBase<T>
    {
        public int TotemBattlePower { get; set; }
        public int SharedBattlePower { get; set; }
        public int TotemDonation { get; set; }
        public int TotemPoleAmount { get; set; }
        public List<TotemPoleStruct> Items { get; set; } = new List<TotemPoleStruct>();

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgTotemPoleInfo);
            writer.Write(0);
            writer.Write(TotemBattlePower);
            writer.Write(TotemDonation);
            writer.Write(SharedBattlePower);
            writer.Write(TotemPoleAmount = Items.Count);
            foreach (var pole in Items)
            {
                writer.Write(pole.Type);
                writer.Write(pole.BattlePower);
                writer.Write(pole.Enhancement);
                writer.Write(pole.Donation);
                writer.Write(pole.Open ? 1 : 0);
            }
            return writer.ToArray();
        }

        public struct TotemPoleStruct
        {
            public int Type;
            public int BattlePower;
            public int Enhancement;
            public long Donation;
            public bool Open;
        }
    }
}
