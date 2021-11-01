using Comet.Game.States;
using Comet.Network.Packets;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comet.Game.Packets
{
    public sealed class MsgSubPro : MsgBase<Client>
    {
        public int Timestamp { get; set; }
        public SubClassActions Action { get; set; }
        public ulong Points { get; set; }
        public SubClasses Class 
        {
            get => (SubClasses)Points;
            set => Points = (ulong)value; 
        }
        public ulong Study { get; set; }
        public byte Count { get; set; }
        public List<AstProfStruct> Professions { get; set; } = new List<AstProfStruct>();

        public override void Decode(byte[] bytes)
        {
            PacketReader reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Timestamp = reader.ReadInt32();
            Action = (SubClassActions)reader.ReadUInt16();
            Points = reader.ReadUInt64();
            Study = reader.ReadUInt64();
            Count = reader.ReadByte();
            for (int i = 0; i < Count; i++)
            {
                Professions.Add(new AstProfStruct
                {
                    Class = (SubClasses)reader.ReadByte(),
                    Level = reader.ReadByte(),
                    Phase = reader.ReadByte()
                });
            }
        }

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort) PacketType.MsgSubPro);
            writer.Write(Timestamp);
            writer.Write((ushort) Action);
            writer.Write(Points);
            writer.Write(Study);
            writer.Write(Count = (byte) Professions.Count);
            for (int i = 0; i < Professions.Count; i++)
            {
                writer.Write((byte)Professions[i].Class);
                writer.Write(Professions[i].Level);
                writer.Write(Professions[i].Phase);
            }
            return writer.ToArray();
        }

        public override Task ProcessAsync(Client client)
        {
            return base.ProcessAsync(client);
        }

        public struct AstProfStruct
        {
            public SubClasses Class { get; set; }
            public byte Level { get; set; }
            public byte Phase { get; set; }
        }

        public enum SubClassActions : ushort
        {
            Switch = 0,
            Activate = 1,
            RequestUplev = 2,
            MartialUplev = 3,
            Learn = 4,
            MartialPromoted = 5,
            Info = 6,
            ShowGui = 7,
            UpdateStudy = 8,
            LearnRemote = 9
        }

        public enum SubClasses : byte
        {
            None = 0,
            MartialArtist = 1,
            Warlock = 2,
            ChiMaster = 3,
            Sage = 4,
            Apothecary = 5,
            Performer = 6,
            Wrangler = 9
        }
    }
}
