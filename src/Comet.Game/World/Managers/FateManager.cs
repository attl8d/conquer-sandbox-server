using Comet.Game.Database.Models;
using Comet.Game.Database.Repositories;
using Comet.Game.Packets;
using Comet.Game.States;
using Comet.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Comet.Game.Packets.MsgRank;
using static Comet.Game.Packets.MsgTrainingVitality;
using static Comet.Game.States.Fate;

namespace Comet.Game.World.Managers
{
    /// <remarks>
    /// Chi System.
    /// </remarks>
    public static class FateManager
    {
        private static ConcurrentDictionary<uint, FateData> mFatePlayers = new ConcurrentDictionary<uint, FateData>();
        private static List<DbFateRank> mFateRanks = new List<DbFateRank>();
        private static List<DbFateRand> mFateRands = new List<DbFateRand>();
        private static List<DbFateRule> mFateRules = new List<DbFateRule>();
        private static List<DbInitFateAttrib> mInitFateAttribs = new List<DbInitFateAttrib>();
        private static List<DbConfig> mFateOpenRule = new List<DbConfig>();
        private static DbConfig mSpendPointsValue = null;
        private static DbConfig mAwardPointsValue = null;

        public static async Task<bool> InitializeAsync()
        {
            await Log.WriteLogAsync("Initializing Fate tables");
            foreach (var player in await DbFatePlayer.GetAsync())
            {
                var user = await CharactersRepository.FindByIdentityAsync(player.PlayerId);
                if (user == null)
                    continue;

                var mate = await CharactersRepository.FindByIdentityAsync(user.Mate);

                var data = new FateData(player)
                {
                    Name = user.Name,
                    Level = user.Level,
                    Mate = mate?.Name ?? Language.StrNone,
                    Profession = user.Profession,
                    FirstProfession = user.FirstProfession,
                    PreviousProfession = user.PreviousProfession,
                    Lookface = user.Mesh
                };
                mFatePlayers.TryAdd(player.PlayerId, data);
            }
            mFateRands.AddRange(await DbFateRand.GetAsync());
            mFateRanks.AddRange(await DbFateRank.GetAsync());
            mFateRules.AddRange(await DbFateRule.GetAsync());
            mInitFateAttribs.AddRange(await DbInitFateAttrib.GetAsync());
            mSpendPointsValue = (await DbConfig.GetAsync(90004)).FirstOrDefault();
            mAwardPointsValue = (await DbConfig.GetAsync(90005)).FirstOrDefault();
            mFateOpenRule.AddRange(await DbConfig.GetAsync(90006));
            return true;
        }

        public static async Task<FateData> GetPlayerAsync(uint idUser)
        {
            if (mFatePlayers.TryGetValue(idUser, out var player))
            {
                return player;
            }

            var dbPlayer = await DbFatePlayer.GetAsync(idUser);
            player = new FateData(dbPlayer);
            if (dbPlayer is not null)
                mFatePlayers.TryAdd(idUser, player);
            return player;
        }

        public static bool Add(FateData fate)
        {
            return mFatePlayers.TryAdd(fate.Identity, fate);
        }

        public static async Task<bool> GenerateAsync(Character user, FateType type, DbFatePlayer fate, TrainingSave flag)
        {
            DbFatePlayer backup = new DbFatePlayer
            {
                Fate1Attrib1 = fate.Fate1Attrib1,
                Fate1Attrib2 = fate.Fate1Attrib2,
                Fate1Attrib3 = fate.Fate1Attrib3,
                Fate1Attrib4 = fate.Fate1Attrib4,
                Fate2Attrib1 = fate.Fate2Attrib1,
                Fate2Attrib2 = fate.Fate2Attrib2,
                Fate2Attrib3 = fate.Fate2Attrib3,
                Fate2Attrib4 = fate.Fate2Attrib4,
                Fate3Attrib1 = fate.Fate3Attrib1,
                Fate3Attrib2 = fate.Fate3Attrib2,
                Fate3Attrib3 = fate.Fate3Attrib3,
                Fate3Attrib4 = fate.Fate3Attrib4,
                Fate4Attrib1 = fate.Fate4Attrib1,
                Fate4Attrib2 = fate.Fate4Attrib2,
                Fate4Attrib3 = fate.Fate4Attrib3,
                Fate4Attrib4 = fate.Fate4Attrib4
            };

            bool success = true;

            var fateRands = mFateRands.Where(x => x.FateNo == (int)type).OrderBy(x => x.RangeRate).ToArray();
            var rules = mFateRules.Where(x => x.FateNo == (int)type).ToList();
            List<TrainingAttrType> generated = new List<TrainingAttrType>();
            for (int i = 1; i < 5; i++)
            {
                if (((int)flag & (1 << (i - 1))) != 0)
                {
                    generated.Add(ReferenceType(GetPowerByIndex(fate, type, i)));
                    continue;
                }
            }

            for (int i = 1; i < 5; i++)
            {
                if (((int)flag & (1 << (i - 1))) != 0)
                {
                    continue;
                }

                int rand = await Kernel.NextAsync(rules.Count) % rules.Count;
                var rule = rules[rand];
                TrainingAttrType attr = (TrainingAttrType)rule.AttrType;

                rules.RemoveAt(rand);

                if (generated.Any(x => (int)x == (int)attr))
                {
                    generated.Add(attr);
                    i--;
                    continue;
                }

                generated.Add(attr);

                int rate = await Kernel.NextAsync(100000);
                double dRate = await Kernel.NextRateAsync(0.999);
                DbFateRand fateRand = null;
                for (int x = 0; x < fateRands.Length; x++)
                {
                    if (rate < fateRands[x].RangeRate)
                    {
                        fateRand = fateRands[x];
                        break;
                    }
                }

                if (fateRand == null)
                {
                    success = false;
                    break;
                }

                dRate += fateRand.RangeNo;
                if (fateRand.FateNo == 100)
                    dRate = Math.Floor(dRate);

                int delta = (int)((rule.AttribValueMax - rule.AttribValueMin) * (dRate / 100));
                int power = Math.Min(rule.AttribValueMax, Math.Max(rule.AttribValueMin, rule.AttribValueMin + delta));
                power += ((int)attr * 10000);
                if (GetScore(type, power) >= 90)
                {
                    await RoleManager.BroadcastMsgAsync(new MsgTrainingVitalityScore
                    {
                        Name = user.Name,
                        AttrType = type,
                        Power = power
                    });
                }

                SetAttribute(fate, type, i, power);
            }

            if (!success)
            {
                fate.Fate1Attrib1 = backup.Fate1Attrib1;
                fate.Fate1Attrib2 = backup.Fate1Attrib2;
                fate.Fate1Attrib3 = backup.Fate1Attrib3;
                fate.Fate1Attrib4 = backup.Fate1Attrib4;

                fate.Fate2Attrib1 = backup.Fate2Attrib1;
                fate.Fate2Attrib2 = backup.Fate2Attrib2;
                fate.Fate2Attrib3 = backup.Fate2Attrib3;
                fate.Fate2Attrib4 = backup.Fate2Attrib4;

                fate.Fate3Attrib1 = backup.Fate3Attrib1;
                fate.Fate3Attrib2 = backup.Fate3Attrib2;
                fate.Fate3Attrib3 = backup.Fate3Attrib3;
                fate.Fate3Attrib4 = backup.Fate3Attrib4;

                fate.Fate4Attrib1 = backup.Fate4Attrib1;
                fate.Fate4Attrib2 = backup.Fate4Attrib2;
                fate.Fate4Attrib3 = backup.Fate4Attrib3;
                fate.Fate4Attrib4 = backup.Fate4Attrib4;
                return false;
            }

            byte[] lockInfo = BitConverter.GetBytes(fate.AttribLockInfo);
            lockInfo[(int)(type - 1)] = (byte)flag;
            fate.AttribLockInfo = BitConverter.ToUInt32(lockInfo);

            await user.Fate.SaveAsync();
            await user.Fate.SendAsync(true);
            return true;
        }

        public static int GetPower(DbFatePlayer fate, TrainingAttrType attr)
        {
            int result = 0;

            if (ReferenceType(fate.Fate1Attrib1) == attr)
                result += Power(fate.Fate1Attrib1);
            else if (ReferenceType(fate.Fate1Attrib2) == attr)
                result += Power(fate.Fate1Attrib2);
            else if (ReferenceType(fate.Fate1Attrib3) == attr)
                result += Power(fate.Fate1Attrib3);
            else if (ReferenceType(fate.Fate1Attrib4) == attr)
                result += Power(fate.Fate1Attrib4);

            if (ReferenceType(fate.Fate2Attrib1) == attr)
                result += Power(fate.Fate2Attrib1);
            else if (ReferenceType(fate.Fate2Attrib2) == attr)
                result += Power(fate.Fate2Attrib2);
            else if (ReferenceType(fate.Fate2Attrib3) == attr)
                result += Power(fate.Fate2Attrib3);
            else if (ReferenceType(fate.Fate2Attrib4) == attr)
                result += Power(fate.Fate2Attrib4);

            if (ReferenceType(fate.Fate3Attrib1) == attr)
                result += Power(fate.Fate3Attrib1);
            else if (ReferenceType(fate.Fate3Attrib2) == attr)
                result += Power(fate.Fate3Attrib2);
            else if (ReferenceType(fate.Fate3Attrib3) == attr)
                result += Power(fate.Fate3Attrib3);
            else if (ReferenceType(fate.Fate3Attrib4) == attr)
                result += Power(fate.Fate3Attrib4);

            if (ReferenceType(fate.Fate4Attrib1) == attr)
                result += Power(fate.Fate4Attrib1);
            else if (ReferenceType(fate.Fate4Attrib2) == attr)
                result += Power(fate.Fate4Attrib2);
            else if (ReferenceType(fate.Fate4Attrib3) == attr)
                result += Power(fate.Fate4Attrib3);
            else if (ReferenceType(fate.Fate4Attrib4) == attr)
                result += Power(fate.Fate4Attrib4);

            return result;
        }

        public static int GetPowerByIndex(DbFatePlayer fate, FateType type, int num)
        {
            if (type == FateType.Dragon)
            {
                if (num == 1)
                    return fate.Fate1Attrib1;
                if (num == 2)
                    return fate.Fate1Attrib2;
                if (num == 3)
                    return fate.Fate1Attrib3;
                if (num == 4)
                    return fate.Fate1Attrib4;
            }
            else if (type == FateType.Phoenix)
            {
                if (num == 1)
                    return fate.Fate2Attrib1;
                if (num == 2)
                    return fate.Fate2Attrib2;
                if (num == 3)
                    return fate.Fate2Attrib3;
                if (num == 4)
                    return fate.Fate2Attrib4;
            }
            else if (type == FateType.Tiger)
            {
                if (num == 1)
                    return fate.Fate3Attrib1;
                if (num == 2)
                    return fate.Fate3Attrib2;
                if (num == 3)
                    return fate.Fate3Attrib3;
                if (num == 4)
                    return fate.Fate3Attrib4;
            }
            else if (type == FateType.Turtle)
            {
                if (num == 1)
                    return fate.Fate4Attrib1;
                if (num == 2)
                    return fate.Fate4Attrib2;
                if (num == 3)
                    return fate.Fate4Attrib3;
                if (num == 4)
                    return fate.Fate4Attrib4;
            }
            return 0;
        }

        private static void SetAttribute(DbFatePlayer fate, FateType type, int num, int value)
        {
            if (type == FateType.Dragon)
            {
                if (num == 1)
                    fate.Fate1Attrib1 = value;
                if (num == 2)
                    fate.Fate1Attrib2 = value;
                if (num == 3)
                    fate.Fate1Attrib3 = value;
                if (num == 4)
                    fate.Fate1Attrib4 = value;
            }
            else if (type == FateType.Phoenix)
            {
                if (num == 1)
                    fate.Fate2Attrib1 = value;
                if (num == 2)
                    fate.Fate2Attrib2 = value;
                if (num == 3)
                    fate.Fate2Attrib3 = value;
                if (num == 4)
                    fate.Fate2Attrib4 = value;
            }
            else if (type == FateType.Tiger)
            {
                if (num == 1)
                    fate.Fate3Attrib1 = value;
                if (num == 2)
                    fate.Fate3Attrib2 = value;
                if (num == 3)
                    fate.Fate3Attrib3 = value;
                if (num == 4)
                    fate.Fate3Attrib4 = value;
            }
            else if (type == FateType.Turtle)
            {
                if (num == 1)
                    fate.Fate4Attrib1 = value;
                if (num == 2)
                    fate.Fate4Attrib2 = value;
                if (num == 3)
                    fate.Fate4Attrib3 = value;
                if (num == 4)
                    fate.Fate4Attrib4 = value;
            }
        }

        public static int GetScore(uint idUser, FateType type)
        {
            if (mFatePlayers.TryGetValue(idUser, out var player))
                return GetScore(player, type);
            return 0;
        }

        public static int GetScore(DbFatePlayer record, FateType type)
        {
            int total = 0;

            if (record == null)
                return total;

            if (type == FateType.Dragon)
            {
                total += GetScore(type, record.Fate1Attrib1);
                total += GetScore(type, record.Fate1Attrib2);
                total += GetScore(type, record.Fate1Attrib3);
                total += GetScore(type, record.Fate1Attrib4);
            }
            else if (type == FateType.Phoenix)
            {
                total += GetScore(type, record.Fate2Attrib1);
                total += GetScore(type, record.Fate2Attrib2);
                total += GetScore(type, record.Fate2Attrib3);
                total += GetScore(type, record.Fate2Attrib4);
            }
            else if (type == FateType.Tiger)
            {
                total += GetScore(type, record.Fate3Attrib1);
                total += GetScore(type, record.Fate3Attrib2);
                total += GetScore(type, record.Fate3Attrib3);
                total += GetScore(type, record.Fate3Attrib4);
            }
            else if (type == FateType.Turtle)
            {
                total += GetScore(type, record.Fate4Attrib1);
                total += GetScore(type, record.Fate4Attrib2);
                total += GetScore(type, record.Fate4Attrib3);
                total += GetScore(type, record.Fate4Attrib4);
            }
            return total;
        }

        private static int GetScore(FateType type, int attrib)
        {
            var rule = GetRule(type, ReferenceType(attrib));
            int refPower = ReferencePower(rule, attrib);
            double baseValue = rule.AttribValueMax - rule.AttribValueMin;
            return (int)((refPower / baseValue) * 100);
        }

        public static TrainingAttrType ReferenceType(int power)
        {
            return (TrainingAttrType)(power / 10000);
        }

        public static int ReferencePower(DbFateRule rule, int power)
        {
            return (power % 10000) - rule.AttribValueMin;
        }

        public static int Power(int power)
        {
            return power % 10000;
        }

        public static async Task SendRankAsync(Character sender, MsgRank msg)
        {
            List<RankData> rankData = new List<RankData>();
            int position = -1;
            FateType type = FateType.None;
            switch (msg.RankMode)
            {
                case RankType.ChiDragon:
                    {
                        rankData = mFatePlayers.Values.OrderByDescending(x => GetScore(x, FateType.Dragon)).Take(50).Select(p => new RankData
                        {
                            Identity = p.Identity,
                            Name = p.Name,
                            Profession = p.Profession,
                            Level = p.Level,
                            LookFace = p.Lookface
                        }).ToList();
                        type = FateType.Dragon;
                        break;
                    }

                case RankType.ChiPhoenix:
                    {
                        rankData = mFatePlayers.Values.OrderByDescending(x => GetScore(x, FateType.Phoenix)).Take(50).Select(p => new RankData
                        {
                            Identity = p.Identity,
                            Name = p.Name,
                            Profession = p.Profession,
                            Level = p.Level,
                            LookFace = p.Lookface
                        }).ToList();
                        type = FateType.Phoenix;
                        break;
                    }

                case RankType.ChiTiger:
                    {
                        rankData = mFatePlayers.Values.OrderByDescending(x => GetScore(x, FateType.Tiger)).Take(50).Select(p => new RankData
                        {
                            Identity = p.Identity,
                            Name = p.Name,
                            Profession = p.Profession,
                            Level = p.Level,
                            LookFace = p.Lookface
                        }).ToList();
                        type = FateType.Tiger;
                        break;
                    }

                case RankType.ChiTurtle:
                    {
                        rankData = mFatePlayers.Values.OrderByDescending(x => GetScore(x, FateType.Turtle)).Take(50).Select(p => new RankData
                        {
                            Identity = p.Identity,
                            Name = p.Name,
                            Profession = p.Profession,
                            Level = p.Level,
                            LookFace = p.Lookface
                        }).ToList();
                        type = FateType.Turtle;
                        break;
                    }
                default:
                    return;
            }

            position = rankData.FindIndex(x => x.Identity == sender.Identity);

            int idx = msg.PageNumber * 10;
            if (idx >= rankData.Count)
                return;

            msg.Data1 = (ushort)rankData.Count;
            if (position > -1 && position < 50)
            {
                msg.Rank.Add(new RankData
                {
                    Position = position + 1,
                    Identity = rankData[position].Identity,
                    Amount = GetScore(sender.Identity, type),
                    Level = rankData[position].Level,
                    LookFace = rankData[position].LookFace,
                    Name = rankData[position].Name,
                    Profession = rankData[position].Profession
                });
            }

            int count = 0;
            for (; idx < rankData.Count && count < 10; idx++, count++)
            {
                msg.Rank.Add(new RankData
                {
                    Position = idx + 1,
                    Identity = rankData[idx].Identity,
                    Amount = GetScore(rankData[idx].Identity, type),
                    Level = rankData[idx].Level,
                    LookFace = rankData[idx].LookFace,
                    Name = rankData[idx].Name,
                    Profession = rankData[idx].Profession
                });
            }
            await sender.SendAsync(msg);
        }

        public static DbFateRule GetRule(FateType type, TrainingAttrType attrType)
        {
            return mFateRules.FirstOrDefault(x => x.FateNo == (int)type && x.AttrType == (int) attrType);
        }

        public static DbConfig GetInitializationRequirements(FateType type)
        {
            return mFateOpenRule.FirstOrDefault(x => x.Data1 == (int) type);
        }
    }

    public class FateData
    {
        private DbFatePlayer mFate;

        public FateData(DbFatePlayer fate)
        {
            mFate = fate;
        }

        public uint Identity => mFate.PlayerId;
        public uint Lookface { get; set; }
        public string Name { get; set; }
        public string Mate { get; set; }
        public int Level { get; set; }
        public int Profession { get; set; }
        public int PreviousProfession { get; set; }
        public int FirstProfession { get; set; }

        public static implicit operator DbFatePlayer(FateData fate)
        {
            return fate.mFate;
        }
    }
}
