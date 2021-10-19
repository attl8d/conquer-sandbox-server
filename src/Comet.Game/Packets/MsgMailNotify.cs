using Comet.Game.States;
using Comet.Network.Packets;

namespace Comet.Game.Packets
{
    public sealed class MsgMailNotify : MsgBase<Client>
    {
        public MailNotification Action { get; set; }

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgMailNotify);
            writer.Write((int) Action);
            return writer.ToArray();
        }

        public enum MailNotification : ushort
        {
            DeletionFailed = 1,
            Notification = 3,
            UnreadMail = 4
        }
    }
}
