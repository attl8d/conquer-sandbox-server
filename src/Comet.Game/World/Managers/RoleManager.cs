// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) FTW! Masters
// Keep the headers and the patterns adopted by the project. If you changed anything in the file just insert
// your name below, but don't remove the names of who worked here before.
// 
// This project is a fork from Comet, a Conquer Online Server Emulator created by Spirited, which can be
// found here: https://gitlab.com/spirited/comet
// 
// Comet - Comet.Game - Role Manager.cs
// Description:
// 
// Creator: FELIPEVIEIRAVENDRAMI [FELIPE VIEIRA VENDRAMINI]
// 
// Developed by:
// Felipe Vieira Vendramini <felipevendramini@live.com>
// 
// Programming today is a race between software engineers striving to build bigger and better
// idiot-proof programs, and the Universe trying to produce bigger and better idiots.
// So far, the Universe is winning.
// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#region References

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using Comet.Core;
using Comet.Game.Database;
using Comet.Game.Database.Models;
using Comet.Game.Database.Repositories;
using Comet.Game.Internal;
using Comet.Game.Packets;
using Comet.Game.States;
using Comet.Game.States.BaseEntities;
using Comet.Game.States.Items;
using Comet.Game.States.Magics;
using Comet.Network.Packets;
using Comet.Network.Packets.Internal;
using Comet.Shared;
using Microsoft.VisualStudio.Threading;

#endregion

namespace Comet.Game.World.Managers
{
    public static class RoleManager
    {
        private static readonly ConcurrentDictionary<uint, Character> m_userSet = new ConcurrentDictionary<uint, Character>();
        private static readonly ConcurrentDictionary<uint, Role> m_roleSet = new ConcurrentDictionary<uint, Role>();
        private static readonly ConcurrentDictionary<uint, MapItem> m_mapItemSet = new ConcurrentDictionary<uint, MapItem>();

        private static readonly Dictionary<byte, DbLevelExperience> m_dicLevExp = new Dictionary<byte, DbLevelExperience>();
        private static readonly Dictionary<uint, DbPointAllot> m_dicPointAllot = new Dictionary<uint, DbPointAllot>();
        private static readonly Dictionary<uint, DbMonstertype> m_dicMonstertype = new Dictionary<uint, DbMonstertype>();
        private static readonly Dictionary<uint, List<DbMonsterMagic>> m_dicMonsterMagics = new Dictionary<uint, List<DbMonsterMagic>>();
        private static readonly Dictionary<uint, DbSuperman> m_superman = new Dictionary<uint, DbSuperman>();
        private static readonly Dictionary<uint, DbAchievementType> m_achievements = new Dictionary<uint, DbAchievementType>();

        private static readonly List<DbRebirth> m_dicRebirths = new List<DbRebirth>();
        private static readonly List<MagicTypeOp> m_magicOps = new List<MagicTypeOp>();
        private static readonly List<DbDisdain> m_disdains = new List<DbDisdain>();
        private static readonly List<DbTutorType> m_tutorType = new List<DbTutorType>();
        private static readonly List<DbTutorBattleLimitType> m_tutorBattleLimitTypes = new List<DbTutorBattleLimitType>();

        private static TimeOutMS m_userUpdate = new TimeOutMS(500);

        private static bool m_isShutdown = false;

        public static int OnlineUniquePlayers => m_userSet.Values.Select(x => x.Client.IPAddress).Distinct().Count();
        public static int OnlinePlayers => m_userSet.Count;
        public static int RolesCount => m_roleSet.Count;

        public static int MaxOnlinePlayers { get; private set; }

        public static async Task InitializeAsync()
        {
            await Log.WriteLogAsync("Initializing user tables");
            foreach (var auto in await PointAllotRepository.GetAsync())
            {
                m_dicPointAllot.TryAdd(AllotIndex(auto.Profession, auto.Level), auto);
            }

            foreach (var mob in await MonsterypeRepository.GetAsync())
            {
                m_dicMonstertype.TryAdd(mob.Id, mob);
            }

            foreach (var lev in await LevelExperienceRepository.GetAsync())
            {
                m_dicLevExp.TryAdd(lev.Level, lev);
            }

            m_dicRebirths.AddRange(await RebirthRepository.GetAsync());

            foreach (var dbOp in await MagictypeOpRepository.GetAsync())
            {
                m_magicOps.Add(new MagicTypeOp(dbOp));
            }

            foreach (var magic in await DbMonsterMagic.GetAsync())
            {
                if (!m_dicMonsterMagics.ContainsKey(magic.OwnerIdentity))
                    m_dicMonsterMagics.TryAdd(magic.OwnerIdentity, new List<DbMonsterMagic>());
                m_dicMonsterMagics[magic.OwnerIdentity].Add(magic);
            }

            var superman = (await DbSuperman.GetAsync()).ToDictionary(superman => superman.UserIdentity);
            foreach (var sm in superman)
            {
                if (!m_superman.ContainsKey(sm.Key))
                    m_superman.Add(sm.Key, sm.Value);
            }

            m_disdains.AddRange(await DbDisdain.GetAsync());

            m_tutorType.AddRange(await DbTutorType.GetAsync());

            m_tutorBattleLimitTypes.AddRange(await DbTutorBattleLimitType.GetAsync());

            foreach (var achievement in await DbAchievementType.GetAllAsync())
            {
                if (!m_achievements.ContainsKey(achievement.Identity))
                    m_achievements.TryAdd(achievement.Identity, achievement);
            }

            m_userUpdate.Update();
        }

        public static async Task<bool> LoginUserAsync(Client user)
        {
            if (m_isShutdown)
            {
                await user.SendAsync(new MsgConnectEx(MsgConnectEx.RejectionCode.ServerDown));
                user.Disconnect();
                return false;
            }

            if (m_userSet.TryGetValue(user.Character.Identity, out var concurrent))
            {
                await Log.WriteLogAsync(LogLevel.Info,
                    $"User {user.Character.Identity} {user.Character.Name} tried to login an already connected client.");

                if (user.IPAddress != concurrent.Client.IPAddress)
                {
                    await concurrent.SendAsync(Language.StrAnotherLoginSameIp, MsgTalk.TalkChannel.Talk);
                }
                else
                {
                    await concurrent.SendAsync(Language.StrAnotherLoginOtherIp, MsgTalk.TalkChannel.Talk);
                }

                concurrent.Client.Disconnect();
                user.Disconnect();
                //await KickOutAsync(user.Character.Identity, "logged twice");
                return false;
            }

            if (m_userSet.Count > Kernel.Configuration.MaxConn && user.AccountIdentity >= 10000 &&
                !user.Character.IsGm())
            {
                await user.SendAsync(new MsgConnectEx(MsgConnectEx.RejectionCode.ServerFull));
                await Log.WriteLogAsync(LogLevel.Warning, $"{user.Character.Name} tried to login and server is full.");
                user.Disconnect();
                return false;
            }

            m_userSet.TryAdd(user.Character.Identity, user.Character);
            m_roleSet.TryAdd(user.Character.Identity, user.Character);

            await user.Character.SetLoginAsync();
            
            await Log.WriteLogAsync(LogLevel.Info, $"{user.Character.Name} has logged in.");

            await Kernel.AccountServer.SendAsync(new MsgAccServerPlayerStatus
            {
                ServerName = Kernel.Configuration.ServerName,
                Status = new List<MsgAccServerPlayerStatus<AccountServer>.PlayerStatus>
                {
                    new MsgAccServerPlayerStatus<AccountServer>.PlayerStatus { Identity = user.AccountIdentity, Online = true }
                }
            });

            { // scope to don't create variable externally
                var msg = new MsgAccServerPlayerExchange
                {
                    ServerName = Kernel.Configuration.ServerName
                };
                msg.Data.Add(MsgAccServerPlayerExchange.CreatePlayerData(user.Character));
                await Kernel.AccountServer.SendAsync(msg);
            }

            if (OnlinePlayers > MaxOnlinePlayers)
                MaxOnlinePlayers = OnlinePlayers;
            return true;
        }

        public static void ForceLogoutUser(uint idUser)
        {
            m_userSet.TryRemove(idUser, out _);
            m_roleSet.TryRemove(idUser, out _);
        }

        public static async Task KickOutAsync(uint idUser, string reason = "")
        {
            if (m_userSet.TryGetValue(idUser, out var user))
            {
                await user.SendAsync(string.Format(Language.StrKickout, reason), MsgTalk.TalkChannel.Talk, Color.White);
                user.Client.Disconnect();
                await Log.WriteLogAsync(LogLevel.Info, $"User {user.Name} has been kicked: {reason}");
            }
        }

        public static async Task KickOutAllAsync(string reason = "", bool isShutdown = false)
        {
            if (isShutdown)
                m_isShutdown = true;

            foreach (var user in m_userSet.Values)
            {
                await user.SendAsync(string.Format(Language.StrKickout, reason), MsgTalk.TalkChannel.Talk, Color.White);
                user.Client.Disconnect();

                await Log.WriteLogAsync(LogLevel.Info, $"User {user.Name} has been kicked (kickoutall): {reason}");
            }
        }

        public static Character GetUserByAccount(uint idAccount)
        {
            return m_userSet.Values.FirstOrDefault(x => x.Client.AccountIdentity == idAccount);
        }

        public static Character GetUser(uint idUser)
        {
            return m_userSet.TryGetValue(idUser, out var client) ? client : null;
        }

        public static Character GetUser(string name)
        {
            return m_userSet.Values.FirstOrDefault(x => x.Name == name);
        }

        public static List<Character> QueryUserSetByMap(uint idMap)
        {
            return m_userSet.Values.Where(x => x.MapIdentity == idMap).ToList();
        }

        public static List<Character> QueryUserSet()
        {
            return m_userSet.Values.ToList();
        }

        /// <summary>
        ///     Attention, DO NOT USE to add <see cref="Character" />.
        /// </summary>
        public static bool AddRole(Role role)
        {
            if (role is MapItem item)
                m_mapItemSet.TryAdd(role.Identity, item);
            return m_roleSet.TryAdd(role.Identity, role);
        }

        public static Role GetRole(uint idRole)
        {
            return m_roleSet.TryGetValue(idRole, out var role) ? role : null;
        }

        public static T GetRole<T>(uint idRole) where T : Role
        {
            return m_roleSet.TryGetValue(idRole, out var role) ? role as T : null;
        }

        public static T GetRole<T>(Func<T, bool> predicate) where T : Role
        {
            return m_roleSet.Values
                .Where(x => x is T)
                .Cast<T>()
                .FirstOrDefault(x => predicate != null && predicate(x));
        }

        public static T FindRole<T>(uint idRole) where T : Role
        {
            foreach (var map in MapManager.GameMaps.Values)
            {
                T result = map.QueryRole<T>(idRole);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static T FindRole<T>(Func<T, bool> predicate) where T : Role
        {
            foreach (var map in MapManager.GameMaps.Values)
            {
                T result = map.QueryRole(predicate);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        ///     Attention, DO NOT USE to remove <see cref="Character" />.
        /// </summary>
        public static bool RemoveRole(uint idRole)
        {
            m_mapItemSet.TryRemove(idRole, out _);
            return m_roleSet.TryRemove(idRole, out _);
        }
        
        public static async Task OnUserTimerAsync()
        {
            bool updateUser = m_userUpdate.ToNextTime();
            foreach (var (_, value) in m_userSet)
            {
                try
                {
                    if (value != null)
                    {
                        await value.OnBattleTimerAsync();
                        if (updateUser)
                            await value.OnTimerAsync();
                    }
                }
                catch (Exception ex)
                {
                    await Log.WriteLogAsync("OnUserTimer", LogLevel.Exception, $"Exception thrown: {ex.Message}\n{ex}");
                }
            }
        }

        public static async Task OnRoleTimerAsync()
        {
            foreach (var item in m_mapItemSet.Values)
            {
                if (item.CanDisappear())
                {
                    Kernel.Services.Processor.Queue(item.Map.Partition, async () =>
                    {
                        await item.DisappearAsync();
                        m_mapItemSet.TryRemove(item.Identity, out _);
                    });
                }
            }

            foreach (var trap in m_roleSet.Values.Where(x => x is MapTrap).Cast<MapTrap>())
            {
                await trap.OnTimerAsync();
            }
        }

        public static async Task OnAiTimerAsync()
        {
            foreach (var ai in m_roleSet.Values.Where(x => x is IRoleAi))
            {
                var roleAi = (IRoleAi) ai;
                if (roleAi.IsActive)
                    await ai.OnTimerAsync();
            }
        }
        
        public static Task BroadcastMsgAsync(string message, MsgTalk.TalkChannel channel = MsgTalk.TalkChannel.System,
            Color? color = null)
        {
            foreach (var user in m_userSet.Values)
            {
                user.SendAsync(message, channel, color).ConfigureAwait(false);
            }
            return Task.CompletedTask;
        }

        public static Task BroadcastMsgAsync(IPacket msg, uint ignore = 0)
        {
            foreach (var user in m_userSet.Values)
            {
                if (user.Identity == ignore) continue;
                    user.SendAsync(msg).ConfigureAwait(false);
            }
            return Task.CompletedTask;
        }

        public static DbPointAllot GetPointAllot(ushort profession, ushort level)
        {
            return m_dicPointAllot.TryGetValue(AllotIndex(profession, level), out var point) ? point : null;
        }

        private static uint AllotIndex(ushort prof, ushort level)
        {
            return (uint) ((prof << 16) + level);
        }

        public static DbMonstertype GetMonstertype(uint type)
        {
            return m_dicMonstertype.TryGetValue(type, out var mob) ? mob : null;
        }

        public static DbLevelExperience GetLevelExperience(byte level)
        {
            return m_dicLevExp.TryGetValue(level, out var value) ? value : null;
        }

        public static int GetLevelLimit()
        {
            return m_dicLevExp.Count + 1;
        }

        public static DbRebirth GetRebirth(int profNow, int profNext, int currMete)
        {
            profNow = (profNow / 10 * 1000) + profNow % 10;
            profNext = (profNext / 10 * 1000) + profNext % 10;
            return m_dicRebirths.FirstOrDefault(x => x.NeedProfession == profNow && x.NewProfession == profNext && x.Metempsychosis == currMete);
        }

        public static MagicTypeOp GetMagictypeOp(MagicTypeOp.MagictypeOperation op, int profNow, int profNext, int metempsychosis)
        {
            return m_magicOps.FirstOrDefault(x => x.ProfessionAgo == profNow && x.ProfessionNow == profNext && x.RebirthTime == metempsychosis && x.Operation == op);
        }

        public static List<T> QueryRoleByMap<T>(uint idMap) where T : Role
        {
            return m_roleSet.Values.Where(x => x.MapIdentity == idMap && x is T).Cast<T>().ToList();
        }

        public static List<T> QueryRoleByType<T>() where T : Role
        {
            return m_roleSet.Values.Where(x => x is T).Cast<T>().ToList();
        }

        public static List<DbMonsterMagic> GetMonsterMagics(uint roleType)
        {
            return m_dicMonsterMagics.TryGetValue(roleType, out var result) ? result : new List<DbMonsterMagic>();
        }

        public static Task AddOrUpdateSupermanAsync(uint idUser, int amount)
        {
            if (!m_superman.TryGetValue(idUser, out var superman))
            {
                m_superman.Add(idUser, superman = new DbSuperman
                {
                    UserIdentity = idUser
                });
            }

            superman.Amount = (uint) amount;
            return BaseRepository.SaveAsync(superman);
        }

        public static int GetSupermanPoints(uint idUser)
        {
            return (int) (m_superman.TryGetValue(idUser, out var value) ? value.Amount : 0);
        }

        public static int GetSupermanRank(uint idUser)
        {
            int result = 1;
            foreach (var super in m_superman.Values.OrderByDescending(x => x.Amount))
            {
                if (super.UserIdentity == idUser)
                    return result;
                result++;
            }
            
            return result;
        }

        public static DbDisdain GetDisdain(int delta)
        {
            return m_disdains.Aggregate((x, y) => Math.Abs(x.DeltaLev - delta) < Math.Abs(y.DeltaLev - delta) ? x : y);
        }

        public static DbTutorBattleLimitType GetTutorBattleLimitType(int delta)
        {
            return m_tutorBattleLimitTypes.Aggregate((x, y) => Math.Abs(x.Id - delta) < Math.Abs(y.Id - delta) ? x : y);
        }

        public static DbTutorType GetTutorType(int level)
        {
            return m_tutorType.FirstOrDefault(x => level >= x.UserMinLevel && level <= x.UserMaxLevel);
        }

        public static DbAchievementType GetAchievementType(int type)
        {
            return m_achievements.Values.FirstOrDefault(x => x.Position == type);
        }
    }
}