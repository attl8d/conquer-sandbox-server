using Comet.Game.Database.Models;
using Comet.Game.States;
using Comet.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Comet.Game.Packets.MsgSubPro;

namespace Comet.Game.World.Managers
{
    public static class AstProfManager
    {
        private static List<DbAstProfInaugurationCondition> mAstProfInit = new List<DbAstProfInaugurationCondition>();
        private static List<DbAstProfPromoteCondition> mDictAstProfPromote = new List<DbAstProfPromoteCondition> ();

        private static readonly int[,] CultivationUpgradePrice =
        {
            { 0, 300, 900, 1800, 2700, 3600, 5100, 6900, 8700, 0 }, // Martial 1
            { 0, 300, 900, 1800, 2700, 3600, 5100, 6900, 8700, 0 }, // Warlock 2
            { 0, 600, 1800, 3600, 5400, 7200, 10200, 13800, 17400, 0 }, // Chi Master 3
            { 0, 400, 1200, 2400, 3600, 4800, 6800, 9200, 11600, 0 }, // Sage 4
            { 0, 100, 200, 300, 400, 500, 1000, 4000, 9000, 0 }, // Apothecary 5
            { 0, 400, 1200, 2400, 3600, 4800, 6800, 9200, 11600, 0 }, // Performer 6
            { 0, 000, 0000, 0000, 0000, 0000, 0000, 0000, 00000, 0 }, // 7
            { 0, 000, 0000, 0000, 0000, 0000, 0000, 0000, 00000, 0 }, // 8
            { 0, 400, 1200, 2400, 3600, 4800, 6800, 9200, 11600, 0 } // Wrangler // 9
        };

        private static readonly int[,] AstProfPower =
        {
            {0, 100, 200, 300, 400, 600, 800, 1000, 1200, 1500, 0},
            {0, 100, 200, 300, 400, 600, 800, 1000, 1200, 1500, 0},
            {0, 100, 200, 300, 400, 600, 800, 1000, 1200, 1500, 0},
            {0, 100, 200, 300, 400, 600, 800, 1000, 1200, 1500, 0},
            {0, 008, 016, 024, 032, 040, 048, 0056, 0064, 0072, 0},
            {0, 100, 200, 300, 400, 500, 600, 0700, 0800, 1000, 0},
            {0, 000, 000, 000, 000, 000, 000, 0000, 0000, 0000, 0}, // 7
            {0, 000, 000, 000, 000, 000, 000, 0000, 0000, 0000, 0}, // 8
            {0, 100, 200, 300, 400, 500, 600, 0800, 1000, 1200, 0}
        };

        public static async Task<bool> InitializeAsync()
        {
            await Log.WriteLogAsync("Initializing Subclasses tables");

            foreach (var inaug in await DbAstProfInaugurationCondition.GetAsync())
            {
                mAstProfInit.Add(inaug);
            }

            foreach (var promote in await DbAstProfPromoteCondition.GetAsync())
            {
                mDictAstProfPromote.Add(promote);
            }

            return true;
        }

        public static async Task<bool> CanInaugurateAsync(Character user, AstProfType type)
        {
            var astProfInit = mAstProfInit.FirstOrDefault(x => x.AstProfType == (byte) type);
            if (astProfInit == null)
                return false;

            if (astProfInit.Metempsychosis > user.Metempsychosis)
                return false;
            if (astProfInit.UserLevel > user.Level && astProfInit.Metempsychosis > user.Metempsychosis)
                return false;
            
            int count1 = (int)Math.Max(1, astProfInit.ItemType1Amount);
            if (astProfInit.ItemType1 != 0)
            {
                if (!user.UserPackage.MultiCheckItem(astProfInit.ItemType1, astProfInit.ItemType1, count1))
                    return false;
            }

            int count2 = (int)Math.Max(1, astProfInit.ItemType2Amount);
            if (astProfInit.ItemType2 != 0)
            {
                
                if (!user.UserPackage.MultiCheckItem(astProfInit.ItemType2, astProfInit.ItemType2, count2))
                    return false;
            }

            if (astProfInit.ItemType1 != 0)
                await user.UserPackage.MultiSpendItemAsync(astProfInit.ItemType1, astProfInit.ItemType1, count1);
            if (astProfInit.ItemType1 != 0)
                await user.UserPackage.MultiSpendItemAsync(astProfInit.ItemType2, astProfInit.ItemType2, count2);

            return true;
        }

        public static async Task<bool> CanUpLevAsync(Character user, AstProfType type, int currentLevel)
        {
            if (currentLevel >= 9)
                return false;

            if (!await user.SpendCultivationAsync(CultivationUpgradePrice[((int) type) - 1, currentLevel]))
                return false;

            return true;
        }

        public static bool CanPromote(Character user, AstProfType type, byte level)
        {
            int rank = GetRank(user, type) + 1;
            if (rank > 9)
                return false;
            var astProfPromote = mDictAstProfPromote.FirstOrDefault(x => x.AstProfType == (byte)type && x.AstProfRank == rank);
            if (astProfPromote == null || astProfPromote.AstProfLevel > level)
                return false;
            if (astProfPromote.Metempsychosis > user.Metempsychosis)
                return false;
            if (astProfPromote.UserLevel > user.Level && astProfPromote.Metempsychosis > user.Metempsychosis)
                return false;
            return true;
        }

        public static byte GetRank(Character user, AstProfType type)
        {
            byte[] ranks = BitConverter.GetBytes(user.AstProfRanks);
            int idx = Math.Max(0, Math.Min(6, (int) type));
            return ranks[idx];
        }

        public static void SetRank(Character user, AstProfType type, byte value)
        {
            byte[] ranks = BitConverter.GetBytes(user.AstProfRanks);
            int idx = Math.Max(0, Math.Min(6, (int)type));
            ranks[idx] = value;
            user.AstProfRanks = BitConverter.ToUInt64(ranks);
        }

        public static int GetPower(AstProfType type, int level)
        {
            switch (type)
            {
                case AstProfType.ChiMaster:
                case AstProfType.Sage:
                case AstProfType.Apothecary:
                case AstProfType.Performer:
                case AstProfType.Wrangler:
                case AstProfType.MartialArtist:
                case AstProfType.Warlock:
                    return AstProfPower[(int)type - 1, level];
                default: return 0;
            }
        }
    }
}
