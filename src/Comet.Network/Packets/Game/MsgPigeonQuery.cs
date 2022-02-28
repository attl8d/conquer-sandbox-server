using Comet.Network.Packets;
using System.Collections.Generic;

namespace Comet.Network.Packets.Game
{
    public abstract class MsgPigeonQuery<T> : MsgBase<T>
    {
        public uint Mode { get; set; }
        public ushort Total { get; set; }
        public ushort Count { get; set; }
        public List<PigeonMessage> Messages = new List<PigeonMessage>();

        public override void Decode(byte[] bytes)
        {
            PacketReader reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Mode = reader.ReadUInt32();
            Total = reader.ReadUInt16();
            Count = reader.ReadUInt16();

            for (int i = 0; i < Count; i++)
            {
                PigeonMessage message = new PigeonMessage();
                message.Identity = reader.ReadUInt32();
                message.Position = reader.ReadUInt32();
                message.UserIdentity = reader.ReadUInt32();
                message.UserName = reader.ReadString(16);
                message.Addition = reader.ReadUInt32();
                message.Message = reader.ReadString(80);
                Messages.Add(message);
            }
        }

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgPigeonQuery);
            writer.Write(Mode);
            writer.Write(Total);
            writer.Write(Count = (ushort)Messages.Count);

            foreach (var message in Messages)
            {
                writer.Write(message.Identity);
                writer.Write(message.Position);
                writer.Write(message.UserIdentity);
                writer.Write(message.UserName, 16);
                writer.Write(message.Addition);
                writer.Write(message.Message, 80);
            }

            return writer.ToArray();
        }

        public struct PigeonMessage
        {
            public uint Identity { get; set; }
            public uint Position { get; set; }
            public uint UserIdentity { get; set; }
            public string UserName { get; set; }
            public uint Addition { get; set; }
            public string Message { get; set; }
        }
    }
}
