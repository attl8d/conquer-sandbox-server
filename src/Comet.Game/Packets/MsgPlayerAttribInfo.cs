using Comet.Game.States;
using Comet.Network.Packets;
using System;
using System.Threading.Tasks;

namespace Comet.Game.Packets
{
    public sealed class MsgPlayerAttribInfo : MsgBase<Client>
    {
        public MsgPlayerAttribInfo()
        {
            Timestamp = Environment.TickCount;
        }

        public MsgPlayerAttribInfo(Character user)
        {
            Timestamp = Environment.TickCount;
            Identity = user.Identity;
            Life = (int)user.Life;
            Mana = (int)user.Mana;
            MaxAttack = user.MaxAttack;
            MinAttack = user.MinAttack;
            PhysicalDefense = user.Defense;
            MagicalAttack = user.MagicAttack;
            MagicalDefense = user.MagicDefense;
            Dodge = user.Dodge;
            Agility = user.AttackSpeed;
            Accuracy = user.Accuracy;
            DragonGemBonus = user.DragonGemBonus;
            PhoenixGemBonus = user.PhoenixGemBonus;
            MagicDefenseBonus = user.MagicDefenseBonus;
            TortoiseGemBonus = user.TortoiseGemBonus;
            Blessing = user.Blessing;

            

            FinalPhysicalDamage = user.AddFinalAttack;
            FinalPhysicalDefense = user.AddFinalDefense;
            FinalMagicalDamage = user.AddFinalMAttack;
            FinalMagicalDefense = user.AddFinalMDefense;
        }

        public int Timestamp { get; set; }
        public uint Identity { get; set; }
        public int Life { get; set; }
        public int Mana { get; set; }
        public int MaxAttack { get; set; }
        public int MinAttack { get; set; }
        public int PhysicalDefense { get; set; }
        public int MagicalAttack { get; set; }
        public int MagicalDefense { get; set; }
        public int Dodge { get; set; }
        public int Agility { get; set; }
        public int Accuracy { get; set; }
        public int DragonGemBonus { get; set; }
        public int PhoenixGemBonus { get; set; }
        public int MagicDefenseBonus { get; set; }
        public int TortoiseGemBonus { get; set; }
        public int Blessing { get; set; }
        public int CriticalStrike { get; set; }
        public int SkillCriticalStrike { get; set; }
        public int Immunity { get; set; }
        public int Penetration { get; set; }
        public int Block { get; set; }
        public int Breakthrough { get; set; }
        public int Counteraction { get; set; }
        public int Detoxication { get; set; }
        public int FinalPhysicalDamage { get; set; }
        public int FinalMagicalDamage { get; set; }
        public int FinalPhysicalDefense { get; set; }
        public int FinalMagicalDefense { get; set; }
        public int MetalDefense { get; set; }
        public int WoodDefense { get; set; }
        public int WaterDefense { get; set; }
        public int FireDefense { get; set; }
        public int EarthDefense { get; set; }

        public override void Decode(byte[] bytes)
        {
            PacketReader reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Timestamp = reader.ReadInt32();
            Identity = reader.ReadUInt32();
        }

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgPlayerAttribInfo);
            writer.Write(Timestamp); // 4
            writer.Write(Identity); // 8
            writer.Write(Life); // 12
            writer.Write(Mana); // 16
            writer.Write(MaxAttack); // 20
            writer.Write(MinAttack); // 24
            writer.Write(PhysicalDefense); // 28
            writer.Write(MagicalAttack); // 32
            writer.Write(MagicalDefense); // 36
            writer.Write(Dodge); // 40
            writer.Write(Agility); // 44
            writer.Write(Accuracy); // 48
            writer.Write(DragonGemBonus); // 52
            writer.Write(PhoenixGemBonus); // 56
            writer.Write(MagicDefenseBonus); // 60
            writer.Write(TortoiseGemBonus); // 64
            writer.Write(Blessing); // 68
            writer.Write(CriticalStrike); // 72
            writer.Write(SkillCriticalStrike); // 76
            writer.Write(Immunity); // 80
            writer.Write(Penetration); // 84
            writer.Write(Block); // 88
            writer.Write(Breakthrough); // 92
            writer.Write(Counteraction); // 96
            writer.Write(Detoxication); // 100
            writer.Write(FinalPhysicalDamage); // 104
            writer.Write(FinalPhysicalDefense); // 108
            writer.Write(FinalMagicalDamage); // 112
            writer.Write(FinalMagicalDefense); // 116
            writer.Write(MetalDefense); // 120
            writer.Write(WoodDefense); // 124
            writer.Write(WaterDefense); // 128
            writer.Write(FireDefense); // 132
            writer.Write(EarthDefense); // 136
            return writer.ToArray();
        }

        public override Task ProcessAsync(Client client)
        {
            if (Identity == client.Character.Identity)
            {
                return client.SendAsync(new MsgPlayerAttribInfo(client.Character));
            }
            else
            {
                var target = Kernel.RoleManager.GetUser(Identity);
                if (target != null)
                    return client.SendAsync(new MsgPlayerAttribInfo(target));
                return Task.CompletedTask;
            }
        }
    }
}