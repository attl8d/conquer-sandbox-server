using Comet.Game.Database;
using Comet.Game.Database.Models;
using Comet.Game.States;
using Comet.Game.World.Managers;
using Comet.Network.Packets;
using Comet.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comet.Game.Packets
{
    public sealed class MsgSubPro : MsgBase<Client>
    {
        public int Timestamp { get; set; }
        public AstProfAction Action { get; set; }
        public ulong Points { get; set; }
        public AstProfType Class 
        {
            get => (AstProfType)Points;
            set => Points = (ulong)value; 
        }
        public byte Level
        {
            get => BitConverter.GetBytes(Points)[1];
            set
            {
                byte[] val = BitConverter.GetBytes(Points);
                val[1] = value;
                Points = BitConverter.ToUInt64(val);
            }
        }
        public ulong Study { get; set; }
        public ulong AwardedStudy { get; set; }
        public int Count { get; set; }
        public List<AstProfStruct> Professions { get; set; } = new List<AstProfStruct>();

        public override void Decode(byte[] bytes)
        {
            PacketReader reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Timestamp = reader.ReadInt32(); // 4
            Action = (AstProfAction)reader.ReadUInt16(); // 8
            Points = reader.ReadUInt64(); // 10
            Study = reader.ReadUInt64(); // 18
            Count = reader.ReadByte(); // 26
            for (int i = 0; i < Count; i++)
            {
                Professions.Add(new AstProfStruct 
                {
                    Class = (AstProfType)reader.ReadByte(),
                    Level = reader.ReadByte(),
                    Rank = reader.ReadByte()
                });
            }
        }

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort) PacketType.MsgSubPro);
            writer.Write(Timestamp); // 4
            writer.Write((ushort) Action); // 8
            writer.Write(Points); // 10
            writer.Write(Study); // 18
            writer.Write(Count = Professions.Count); // 26
            for (int i = 0; i < Professions.Count; i++)
            {
                writer.Write((byte)Professions[i].Class);
                writer.Write(Professions[i].Level);
                writer.Write(Professions[i].Rank);
            }
            return writer.ToArray();
        }

        public override Task ProcessAsync(Client client)
        {
            switch (Action)
            {
                case AstProfAction.Switch: return client.Character.AstProf.ActivateAsync(Class);
                case AstProfAction.RequestUplev: return client.Character.AstProf.UpLevAsync(Class);
                case AstProfAction.MartialPromoted: return client.Character.AstProf.PromoteAsync(Class);
                case AstProfAction.Info: return client.Character.AstProf.SendAsync();
                case AstProfAction.LearnRemote: return client.Character.AstProf.LearnAsync(Class);
                case AstProfAction.PromoteRemote: return client.Character.AstProf.PromoteAsync(Class);
                default: return Log.WriteLogAsync(LogLevel.Socket, $"MsgSubPro Unhandled action [{Action}].\r\n\t{PacketDump.Hex(Encode())}");
            }
        }

        public struct AstProfStruct
        {
            public AstProfType Class { get; set; }
            public byte Level { get; set; }
            public byte Rank { get; set; }
        }

        public class AstProf
        {
            public AstProf(AstProfType type, uint owner)
            {
                mDb = new DbAstProfLevel
                {
                    AstProf = (byte)type,
                    Level = 1,
                    UserIdentity = owner
                };
            }

            public AstProf(DbAstProfLevel db)
            {
                mDb = db;
            }

            public AstProfType Class { get => (AstProfType)mDb.AstProf; }
            public byte Level { get => mDb.Level; set => mDb.Level = value; }

            private DbAstProfLevel mDb;

            public Task SaveAsync() => BaseRepository.SaveAsync(mDb);
            public Task DeleteAsync() => BaseRepository.DeleteAsync(mDb);
        }

        public enum AstProfAction : ushort
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
            LearnRemote = 9,
            PromoteRemote = 10
        }

        public enum AstProfType : byte
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
