using Comet.Game.States;
using Comet.Network.Packets;
using System.Threading.Tasks;

namespace Comet.Game.Packets
{
    public sealed class MsgMailOperation : MsgBase<Client>
    {
        public Mode Action { get; set; }
        public uint EmailIdentity { get; set; }

        public override void Decode(byte[] bytes)
        {
            PacketReader reader = new (bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Action = (Mode)reader.ReadInt32();
            EmailIdentity = reader.ReadUInt32();
        }

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgMailOperation);
            writer.Write((int)Action);
            writer.Write(EmailIdentity);
            return writer.ToArray();
        }

        public override async Task ProcessAsync(Client client)
        {
            switch (Action)
            {
                case Mode.Open:
                    {
                        await client.Character.MailBox.SendMessageAsync(EmailIdentity);
                        break;
                    }

                case Mode.Delete:
                    {
                        await client.Character.MailBox.DeleteMessageAsync(EmailIdentity);
                        await client.SendAsync(this);
                        break;
                    }

                case Mode.ClaimMoney:
                    {
                        await client.Character.MailBox.ClaimMoneyAsync(EmailIdentity);
                        await client.SendAsync(this);
                        break;
                    }

                case Mode.ClaimConquerPoints:
                    {
                        await client.Character.MailBox.ClaimConquerPointsAsync(EmailIdentity);
                        await client.SendAsync(this);
                        break;
                    }

                case Mode.ClaimItem:
                    {
                        await client.Character.MailBox.ClaimActionAsync(EmailIdentity);
                        await client.SendAsync(this);
                        break;
                    }

                case Mode.ClaimAttachment:
                    {
                        await client.Character.MailBox.ClaimItemAsync(EmailIdentity);
                        await client.SendAsync(this);
                        break;
                    }
            }
        }

        public enum Mode
        {
            None,
            Open,
            Delete,
            ClaimMoney,
            ClaimConquerPoints,
            ClaimItem,
            ClaimAttachment
        }
    }
}