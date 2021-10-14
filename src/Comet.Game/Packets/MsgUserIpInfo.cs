using Comet.Game.States;
using Comet.Network.Packets;

namespace Comet.Game.Packets
{
    public sealed class MsgUserIpInfo : MsgBase<Client>
    {
        public MsgUserIpInfo()
        {
            LoginTime = 0x4e591dba;
        }

        public int LoginTime { get; set; }
        public LastLoginAction Action { get; set; }
        public bool DifferentAddress { get; set; }

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgUserIpInfo);
            writer.Write(LoginTime);
            writer.Write((byte)Action);
            writer.Write(DifferentAddress);
            return writer.ToArray();
        }

        public enum LastLoginAction : byte
        {
            LastLogin,
            DifferentCity,
            DifferentPlace
        }
    }
}