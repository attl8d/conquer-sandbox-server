using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Comet.Core;
using Comet.Database.Entities;
using Comet.Game.Database;
using Comet.Game.Database.Repositories;
using Comet.Game.Packets;
using Comet.Game.World.Managers;
using Comet.Network.Packets;
using Comet.Network.Packets.Game;

namespace Comet.Game.States.Families
{
    public sealed class Family
    {
        public const int MAX_MEMBERS = 6;
        public const int MAX_RELATION = 5;

        private DbFamily m_family;
        private ConcurrentDictionary<uint, FamilyMember> m_members = new ConcurrentDictionary<uint, FamilyMember>();
        private ConcurrentDictionary<uint, Family> m_allies = new ConcurrentDictionary<uint, Family>();
        private ConcurrentDictionary<uint, Family> m_enemies = new ConcurrentDictionary<uint, Family>();

        private Family()
        {

        }

        #region Static Creation

        public static async Task<Family> CreateAsync(Character leader, string name, uint money)
        {
            DbFamily dbFamily = new DbFamily
            {
                Announcement = Language.StrFamilyDefaultAnnounce,
                Amount = 1,
                CreationDate = DateTime.Now,
                LeaderIdentity = leader.Identity,
                Money = money,
                Name = name,
                Rank = 0,
                AllyFamily0 = 0,
                AllyFamily1 = 0,
                AllyFamily2 = 0,
                AllyFamily3 = 0,
                AllyFamily4 = 0,
                EnemyFamily0 = 0,
                EnemyFamily1 = 0,
                EnemyFamily2 = 0,
                EnemyFamily3 = 0,
                EnemyFamily4 = 0,
                Challenge = 0,
                ChallengeMap = 0,
                CreateName = "",
                FamilyMap = 0,
                Occupy = 0,
                Repute = 0,
                StarTower = 0
            };

            if (!await ServerDbContext.SaveAsync(dbFamily))
                return null;

            Family family = new Family
            {
                m_family = dbFamily
            };

            FamilyMember fmLeader = await FamilyMember.CreateAsync(leader, family, FamilyRank.ClanLeader, money);
            if (fmLeader == null)
            {
                await ServerDbContext.DeleteAsync(dbFamily);
                return null;
            }

            family.m_members.TryAdd(fmLeader.Identity, fmLeader);
            FamilyManager.AddFamily(family);

            await leader.SendFamilyAsync();
            await family.SendRelationsAsync(leader);

            await RoleManager.BroadcastMsgAsync(string.Format(Language.StrFamilyCreate, leader.Name, family.Name));
            return family;
        }

        public static async Task<Family> CreateAsync(DbFamily dbFamily)
        {
            Family family = new Family {m_family = dbFamily};

            var members = await FamilyAttrRepository.GetAsync(family.Identity);
            if (members == null)
                return null;

            foreach (var dbMember in members.OrderByDescending(x => x.Rank))
            {
                FamilyMember member = await FamilyMember.CreateAsync(dbMember, family);
                if (member == null)
                {
                    await ServerDbContext.DeleteAsync(dbMember);
                    continue;
                }

                family.m_members.TryAdd(member.Identity, member);
            }

            // validate our members
            foreach (var member in family.m_members.Values.Where(x => x.Rank == FamilyRank.Spouse))
            {
                var mate = family.GetMember(member.MateIdentity);
                if (mate == null || mate.Rank == FamilyRank.Spouse)
                {
                    family.m_members.TryRemove(member.Identity, out _);
                    await member.DeleteAsync();
                }
            }

            return family;
        }

        #endregion

        #region Properties

        public uint Identity => m_family.Identity;
        public string Name => m_family.Name;
        public int MembersCount => m_members.Count;
        public int PureMembersCount => m_members.Count(x => x.Value.Rank != FamilyRank.Spouse);
        public bool IsDeleted => m_family.DeleteDate != null;

        public uint LeaderIdentity => m_family.LeaderIdentity;
        public FamilyMember Leader => m_members.TryGetValue(LeaderIdentity, out var value) ? value : null;

        public ulong Money
        {
            get => m_family.Money;
            set => m_family.Money = value;
        }

        public byte Rank
        {
            get => m_family.Rank;
            set => m_family.Rank = value;
        }

        public uint Reputation
        {
            get => m_family.Repute;
            set => m_family.Repute = value;
        }

        public string Announcement
        {
            get => m_family.Announcement;
            set => m_family.Announcement = value;
        }

        public byte BattlePowerTower
        {
            get => m_family.StarTower;
            set => m_family.StarTower = value;
        }

        public DateTime CreationDate => m_family.CreationDate;

        #endregion

        #region Clan War

        /// <summary>
        /// The map that the family will fight for.
        /// </summary>
        public uint ChallengeMap
        {
            get => m_family.ChallengeMap;
            set => m_family.ChallengeMap = value;
        }

        /// <summary>
        /// The map that the family is currently dominating.
        /// </summary>
        public uint FamilyMap
        {
            get => m_family.FamilyMap;
            set => m_family.FamilyMap = value;
        }

        public uint OccupyDate
        {
            get => m_family.Occupy;
            set => m_family.Occupy = value;
        }

        public uint OccupyDays
        {
            get
            {
                if (OccupyDate != 0)
                {
                    uint days = uint.Parse(DateTime.Now.ToString("yyyyMMdd")) - OccupyDate;
                    if (DateTime.Now.Hour >= 21)
                        return days + 1;
                    return days;
                }
                return 0;
            }
        }

        #endregion

        #region Members

        public int SharedBattlePowerFactor
        {
            get
            {
                switch (BattlePowerTower)
                {
                    case 1: return 40;
                    case 2: return 50;
                    case 3: return 60;
                    case 4: return 70;
                    default:
                        return 30;
                }
            }
        }

        public FamilyMember GetMember(uint idMember)
        {
            return m_members.TryGetValue(idMember, out var value) ? value : null;
        }

        public FamilyMember GetMember(string name) => m_members.Values.FirstOrDefault(x => x.Name.Equals(name));

        public async Task<bool> AppendMemberAsync(Character caller, Character target, FamilyRank rank = FamilyRank.Member)
        {
            if (target.Family != null)
                return false;

            if (target.Level < 50)
                return false;

            if (m_members.Values.Count(x => x.Rank != FamilyRank.Spouse) > 6 && rank != FamilyRank.Spouse)
                return false;

            FamilyMember mate = GetMember(target.MateIdentity);
            if (mate != null)
                return false; // can't rejoin mate clan

            FamilyMember member = await FamilyMember.CreateAsync(target, this, rank);
            if (member == null)
                return false;

            m_members.TryAdd(member.Identity, member);

            target.Family = this;

            if (rank != FamilyRank.Spouse)
            {
                Character mateCharacter = RoleManager.GetUser(target.MateIdentity);
                if (mateCharacter != null)
                    await AppendMemberAsync(caller, mateCharacter, FamilyRank.Spouse);
            }

            await target.SendFamilyAsync();
            await SendRelationsAsync(target);
            await target.Screen.SynchroScreenAsync();
            return true;
        }

        public async Task<bool> LeaveAsync(Character user)
        {
            if (user.Family == null)
                return false;

            if (user.FamilyPosition == FamilyRank.ClanLeader)
                return false;

            if (user.FamilyPosition == FamilyRank.Spouse)
                return false;

            m_members.TryRemove(user.Identity, out _);
            user.Family = null;
            await user.SendNoFamilyAsync();
            await user.Screen.SynchroScreenAsync();

            if (user.MateIdentity != 0)
            {
                FamilyMember mate = GetMember(user.MateIdentity);
                if (mate != null)
                    await KickOutAsync(user, user.MateIdentity);
            }

            return true;
        }

        public async Task<bool> KickOutAsync(Character caller, uint idTarget)
        {
            FamilyMember target = GetMember(idTarget);
            if (target == null)
                return false;

            if (target.Rank == FamilyRank.ClanLeader)
                return false;

            if (caller.FamilyPosition != FamilyRank.ClanLeader)
            {
                if (target.Rank != FamilyRank.Spouse || target.MateIdentity != caller.Identity)
                    return false;
            }

            m_members.TryRemove(idTarget, out _);

            if (target.User != null)
            {
                target.User.Family = null;
                await target.User.SendNoFamilyAsync();
                await target.User.Screen.SynchroScreenAsync();
            }

            await target.DeleteAsync();

            FamilyMember mate = GetMember(target.MateIdentity);
            if (mate != null && mate.Rank == FamilyRank.Spouse)
                await KickOutAsync(caller, mate.Identity);

            return true;
        }

        public async Task<bool> AbdicateAsync(Character caller, string targetName)
        {
            if (caller.FamilyPosition != FamilyRank.ClanLeader)
                return false;

            FamilyMember target = GetMember(targetName);
            if (target == null)
                return false;

            if (caller.Identity == target.Identity)
                return false;

            if (target.FamilyIdentity != Identity)
                return false;

            if (target.User == null)
                return false; // not online

            if (target.Rank == FamilyRank.Spouse)
                return false; // cannot abdicate for a spouse

            target.Rank = FamilyRank.ClanLeader;
            caller.FamilyMember.Rank = FamilyRank.Member;

            await target.SaveAsync();
            await caller.SaveAsync();

            await target.User.SendFamilyAsync();
            await caller.SendFamilyAsync();

            await target.User.Screen.SynchroScreenAsync();
            await caller.Screen.SynchroScreenAsync();

            await RoleManager.BroadcastMsgAsync(string.Format(Language.StrFamilyAbdicate, caller.Name, target.Name), TalkChannel.Family);
            return true;
        }

        #endregion

        #region Relations

        public void LoadRelations()
        {
            // Ally
            var family = FamilyManager.GetFamily(m_family.AllyFamily0);
            if (family != null)
                m_allies.TryAdd(family.Identity, family);
            else m_family.AllyFamily0 = 0;

            family = FamilyManager.GetFamily(m_family.AllyFamily1);
            if (family != null)
                m_allies.TryAdd(family.Identity, family);
            else m_family.AllyFamily1 = 0;

            family = FamilyManager.GetFamily(m_family.AllyFamily2);
            if (family != null)
                m_allies.TryAdd(family.Identity, family);
            else m_family.AllyFamily2 = 0;

            family = FamilyManager.GetFamily(m_family.AllyFamily3);
            if (family != null)
                m_allies.TryAdd(family.Identity, family);
            else m_family.AllyFamily3 = 0;

            family = FamilyManager.GetFamily(m_family.AllyFamily4);
            if (family != null)
                m_allies.TryAdd(family.Identity, family);
            else m_family.AllyFamily4 = 0;

            // Enemies
            family = FamilyManager.GetFamily(m_family.EnemyFamily0);
            if (family != null)
                m_allies.TryAdd(family.Identity, family);
            else m_family.EnemyFamily0 = 0;

            family = FamilyManager.GetFamily(m_family.EnemyFamily1);
            if (family != null)
                m_allies.TryAdd(family.Identity, family);
            else m_family.EnemyFamily1 = 0;

            family = FamilyManager.GetFamily(m_family.EnemyFamily2);
            if (family != null)
                m_allies.TryAdd(family.Identity, family);
            else m_family.EnemyFamily2 = 0;

            family = FamilyManager.GetFamily(m_family.EnemyFamily3);
            if (family != null)
                m_allies.TryAdd(family.Identity, family);
            else m_family.EnemyFamily3 = 0;

            family = FamilyManager.GetFamily(m_family.EnemyFamily4);
            if (family != null)
                m_allies.TryAdd(family.Identity, family);
            else m_family.EnemyFamily4 = 0;
        }

        #endregion

        #region Allies

        public int AllyCount => m_allies.Count;

        public bool IsAlly(uint idAlly) => m_allies.ContainsKey(idAlly);

        public void SetAlly(Family ally)
        {
            uint idAlly = ally.Identity;

            if (m_family.AllyFamily0 == 0)
                m_family.AllyFamily0 = idAlly;
            else if (m_family.AllyFamily1 == 0)
                m_family.AllyFamily1 = idAlly;
            else if (m_family.AllyFamily2 == 0)
                m_family.AllyFamily2 = idAlly;
            else if (m_family.AllyFamily3 == 0)
                m_family.AllyFamily3 = idAlly;
            else if (m_family.AllyFamily4 == 0)
                m_family.AllyFamily4 = idAlly;
            else return;

            m_allies.TryAdd(idAlly, ally);
        }

        public void UnsetAlly(uint idAlly)
        {
            if (m_family.AllyFamily0 == idAlly)
                m_family.AllyFamily0 = 0;

            if (m_family.AllyFamily1 == idAlly)
                m_family.AllyFamily1 = 0;

            if (m_family.AllyFamily2 == idAlly)
                m_family.AllyFamily2 = 0;

            if (m_family.AllyFamily3 == idAlly)
                m_family.AllyFamily3 = 0;

            if (m_family.AllyFamily4 == idAlly)
                m_family.AllyFamily4 = 0;

            m_allies.TryRemove(idAlly, out _);
        }

        #endregion

        #region Enemies

        public int EnemyCount => m_enemies.Count;

        public bool IsEnemy(uint idEnemy) => m_enemies.ContainsKey(idEnemy);

        public void SetEnemy(Family enemy)
        {
            uint idEnemy = enemy.Identity;
            if (m_family.EnemyFamily0 == 0)
                m_family.EnemyFamily0 = idEnemy;
            else if (m_family.EnemyFamily1 == 0)
                m_family.EnemyFamily1 = idEnemy;
            else if (m_family.EnemyFamily2 == 0)
                m_family.EnemyFamily2 = idEnemy;
            else if (m_family.EnemyFamily3 == 0)
                m_family.EnemyFamily3 = idEnemy;
            else if (m_family.EnemyFamily4 == 0)
                m_family.EnemyFamily4 = idEnemy;
            else return;

            m_enemies.TryAdd(idEnemy, enemy);
        }

        public void UnsetEnemy(uint idEnemy)
        {
            if (m_family.EnemyFamily0 == idEnemy)
                m_family.EnemyFamily0 = 0;

            if (m_family.EnemyFamily1 == idEnemy)
                m_family.EnemyFamily1 = 0;

            if (m_family.EnemyFamily2 == idEnemy)
                m_family.EnemyFamily2 = 0;

            if (m_family.EnemyFamily3 == idEnemy)
                m_family.EnemyFamily3 = 0;

            if (m_family.EnemyFamily4 == idEnemy)
                m_family.EnemyFamily4 = 0;

            m_enemies.TryRemove(idEnemy, out _);
        }


        #endregion

        #region Socket

        public Task SendMembersAsync(int idx, Character target)
        {
            if (target.FamilyIdentity != Identity)
                return Task.CompletedTask;

            MsgFamily msg = new MsgFamily
            {
                Identity = Identity,
                Action = MsgFamily.FamilyAction.QueryMemberList
            };

            foreach (var member in m_members.Values.OrderByDescending(x => x.IsOnline).ThenByDescending(x => x.Rank))
            {
                msg.Objects.Add(new MsgFamily.MemberListStruct
                {
                    Profession = member.Profession,
                    Donation = member.Proffer,
                    Name = member.Name,
                    Rank = (ushort) member.Rank,
                    Level = member.Level,
                    Online = member.IsOnline
                });
            }

            return target.SendAsync(msg);
        }

        public async Task SendRelationsAsync()
        {
            foreach (var member in m_members.Values.Where(x => x.IsOnline))
            {
                await SendRelationsAsync(member.User);
            }
        }

        public async Task SendRelationsAsync(Character target)
        {
            MsgFamily msg = new MsgFamily
            {
                Identity = Identity,
                Action = MsgFamily.FamilyAction.SendAlly
            };
            foreach (var ally in m_allies.Values)
                msg.Objects.Add(new MsgFamily.RelationListStruct
                {
                    Name = ally.Name,
                    LeaderName = ally.Leader.Name
                });
            await target.SendAsync(msg);

            msg = new MsgFamily
            {
                Identity = Identity,
                Action = MsgFamily.FamilyAction.SendEnemy
            };
            foreach (var enemy in m_enemies.Values)
                msg.Objects.Add(new MsgFamily.RelationListStruct
                {
                    Name = enemy.Name,
                    LeaderName = enemy.Leader.Name
                });
            await target.SendAsync(msg);
        }

        public async Task SendAsync(string message, uint idIgnore = 0u, Color? color = null)
        {
            await SendAsync(new MsgTalk(0, TalkChannel.Family, color ?? Color.White, message), idIgnore);
        }

        public async Task SendAsync(IPacket msg, uint exclude = 0u)
        {
            foreach (var player in m_members.Values)
            {
                if (exclude == player.Identity || player.User == null)
                    continue;
                await player.User.SendAsync(msg);
            }
        }

        #endregion

        #region Database

        public Task<bool> SaveAsync()
        {
            return ServerDbContext.SaveAsync(m_family);
        }

        public Task<bool> SoftDeleteAsync()
        {
            m_family.DeleteDate = DateTime.Now;
            return SaveAsync();
        }

        public Task<bool> DeleteAsync()
        {
            return ServerDbContext.DeleteAsync(m_family);
        }

        #endregion

        public enum FamilyRank : ushort
        {
            ClanLeader = 100,
            Spouse = 11,
            Member = 10,
            None = 0
        }
    }
}