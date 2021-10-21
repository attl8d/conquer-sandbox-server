using Comet.Game.States;
using Comet.Network.Packets;
using System;
using System.Threading.Tasks;

namespace Comet.Game.Packets
{
    public sealed class MsgNationality : MsgBase<Client>
    {
        public uint Identity { get; set; }
        public PlayerCountry Country { get; set; }

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgNationality);
            writer.Write(Identity);
            writer.Write((ushort)Country);
            return writer.ToArray();
        }

        public override void Decode(byte[] bytes)
        {
            PacketReader reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Identity = reader.ReadUInt32();
            Country = (PlayerCountry)reader.ReadUInt16();
        }

        public override Task ProcessAsync(Client client)
        {
            if (!Enum.IsDefined(typeof(PlayerCountry), Country))
                return Task.CompletedTask;

            client.Character.PlayerCountry = Country;
            Identity = client.Character.Identity;
            return client.Character.BroadcastRoomMsgAsync(this, true);
        }
    }
}
