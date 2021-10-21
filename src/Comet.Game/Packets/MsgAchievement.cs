using Comet.Game.States;
using Comet.Network.Packets;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comet.Game.Packets
{
    public sealed class MsgAchievement : MsgBase<Client>
    {
        public AchievementRequest Action { get; set; }
        public uint Identity { get; set; }
        public int Flag { get; set; }
        public int Count { get; set; }
        public List<uint> Flags { get; set; } = new List<uint>();

        public override byte[] Encode()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgAchievement);
            writer.Write((int)Action);
            writer.Write(Identity);
            writer.Write(Flag);
            writer.Write(Count = Flags.Count);
            foreach (var flag in Flags)
            {
                writer.Write(flag);
            }
            return writer.ToArray();
        }

        public override void Decode(byte[] bytes)
        {
            PacketReader reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Action = (AchievementRequest)reader.ReadInt32();
            Identity = reader.ReadUInt32();
            Flag = reader.ReadInt32();
            Count = reader.ReadInt32();
        }

        public override async Task ProcessAsync(Client client)
        {
            switch (Action)
            {
                case AchievementRequest.Synchro:
                    {
                        Character target = Kernel.RoleManager.GetUser(Identity);
                        if (target != null)
                        {
                            await target.Achievements.SynchroAsync(client.Character);
                        }
                        break;
                    }

                case AchievementRequest.Achieve:
                    {
                        await client.Character.Achievements.AwardAchievementAsync(Flag);
                        break;
                    }
            }
        }

        public enum AchievementRequest
        {
            Synchro = 0,
            Query = 1,
            Achieve = 2
        }
    }
}
