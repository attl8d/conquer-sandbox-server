namespace Comet.Network.Packets.Game
{
    public abstract class MsgWeather<T> : MsgBase<T>
    {
        public uint WeatherType { get; set; }
        public uint Intensity { get; set; }
        public uint Direction { get; set; }
        public uint ColorArgb { get; set; }

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgWeather);
            writer.Write(WeatherType);
            writer.Write(Intensity);
            writer.Write(Direction);
            writer.Write(ColorArgb);
            return writer.ToArray();
        }
    }
}
