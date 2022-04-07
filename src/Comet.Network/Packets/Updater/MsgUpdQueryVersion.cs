namespace Comet.Network.Packets.Updater
{
    public abstract class MsgUpdQueryVersion<T> : MsgBase<T>
    {
        public int Timestamp { get; set; }
        public int ClientVersion { get; set; }
        public int GameVersion { get; set; }

        /// <inheritdoc />
        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort) PacketType.MsgUpdQueryVersion);
            writer.Write(Timestamp);
            writer.Write(ClientVersion);
            writer.Write(GameVersion);
            return writer.ToArray();
        }

        /// <inheritdoc />
        public override void Decode(byte[] bytes)
        {
            PacketReader reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType) reader.ReadUInt16();
            Timestamp = reader.ReadInt32();
            ClientVersion = reader.ReadInt32();
            GameVersion = reader.ReadInt32();
        }
    }
}
