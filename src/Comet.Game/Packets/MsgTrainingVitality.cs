using Comet.Game.States;
using Comet.Game.World.Managers;
using Comet.Network.Packets;
using Comet.Shared;
using System;
using System.Threading.Tasks;

namespace Comet.Game.Packets
{
    public sealed class MsgTrainingVitality : MsgBase<Client>
    {
        public uint Identity { get; set; }
        public TrainingAction Action { get; set; }
        public byte Mode { get; set; }
        public uint Param { get; set; }

        public override void Decode(byte[] bytes)
        {
            PacketReader reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Identity = reader.ReadUInt32();
            Action = (TrainingAction)reader.ReadUInt16();
            Mode = reader.ReadByte();
            Param = reader.ReadUInt32();
        }

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgTrainingVitality);
            writer.Write(Identity);
            writer.Write((ushort)Action);
            writer.Write(Mode);
            writer.Write(Param);
            return writer.ToArray();
        }

        public override Task ProcessAsync(Client client)
        {
            switch (Action)
            {
                case TrainingAction.TrainingtypeUnlock: return client.Character.Fate.UnlockAsync((Fate.FateType)Mode);
                case TrainingAction.TrainingtypeQueryinfo:
                    {
                        if (Identity == client.Character.Identity)
                        {
                            return client.Character.Fate.SendAsync(false);
                        }

                        Character target = RoleManager.GetUser(Identity);
                        if (target == null)
                            return Task.CompletedTask;
                        return target.Fate.SendAsync(false, client.Character);
                    }
                case TrainingAction.TrainingtypeStudy: return client.Character.Fate.GenerateAsync((Fate.FateType)Mode, (TrainingSave)Param);
                default: return Log.WriteLogAsync($"{Type}::{Action} unhandled");
            }
        }

        public enum TrainingAction
        {
            TrainingtypeUnlock = 0,
            TrainingtypeQueryinfo = 1,
            TrainingtypeStudy = 2,
            TrainingtypeBuyStrength = 3,
            TrainingtypeBuyStrength2 = 6
        }

        [Flags]
        public enum TrainingSave
        {
            None = 0,
            Attr1 = 0x1,
            Attr2 = 0x2,
            Attr3 = 0x4,
            Attr4 = 0x8
        }

        public enum TrainingAttrType
        {
            None = 0,
            Criticalstrike = 1,
            Skillcriticalstrike = 2,
            Immunity = 3,
            Breakthrough = 4,
            Counteraction = 5,
            Health = 6,
            Attack = 7,
            Magicattack = 8,
            Mdefense = 9,
            Finalattack = 10,
            Finalmagicattack = 11,
            Damagereduction = 12,
            Magicdamagereduction = 13
        }
    }
}
