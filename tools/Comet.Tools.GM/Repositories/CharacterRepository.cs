using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Comet.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Comet.Tools.GM.Repositories
{
    public static class CharacterRepository
    {
        /// <summary>
        ///     Fetches a character record from the database using the character's name as a
        ///     unique key for selecting a single record. Character name is indexed for fast
        ///     lookup when logging in.
        /// </summary>
        /// <param name="name">Character's name</param>
        /// <returns>Returns character details from the database.</returns>
        public static async Task<DbCharacter> FindAsync(string name)
        {
            await using var db = new ServerDbContext();
            return await db.Characters
                           .Where(x => x.Name == name)
                           .SingleOrDefaultAsync();
        }

        /// <summary>
        ///     Fetches a character record from the database using the character's associated
        ///     AccountID as a unique key for selecting a single record.
        /// </summary>
        /// <param name="accountID">Primary key for fetching character info</param>
        /// <returns>Returns character details from the database.</returns>
        public static async Task<DbCharacter> FindAsync(uint accountID)
        {
            await using var db = new ServerDbContext();
            return await db.Characters
                           .Where(x => x.AccountIdentity == accountID)
                           .SingleOrDefaultAsync();
        }

        /// <summary>Checks if a character exists in the database by name.</summary>
        /// <param name="name">Character's name</param>
        /// <returns>Returns true if the character exists.</returns>
        public static async Task<bool> ExistsAsync(string name)
        {
            await using var db = new ServerDbContext();
            return await db.Characters
                           .Where(x => x.Name == name)
                           .AnyAsync();
        }

        /// <summary>
        ///     Creates a new character using a character model. If the character primary key
        ///     already exists, then character creation will fail.
        /// </summary>
        /// <param name="character">Character model to be inserted to the database</param>
        public static async Task CreateAsync(DbCharacter character)
        {
            await using var db = new ServerDbContext();
            db.Characters.Add(character);
            await db.SaveChangesAsync();
        }

        public static async Task<DbCharacter> FindByIdentityAsync(uint id)
        {
            await using var db = new ServerDbContext();
            return await db.Characters
                           .Where(x => x.Identity == id)
                           .SingleOrDefaultAsync();
        }

        public static async Task<List<DbCharacter>> GetHonorRankAsync(int from, int limit)
        {
            await using var ctx = new ServerDbContext();
            return await ctx.Characters
                            .Where(x => x.AthleteHistoryHonorPoints > 0)
                            .OrderByDescending(x => x.AthleteHistoryHonorPoints)
                            .ThenByDescending(x => x.AthleteHistoryWins)
                            .ThenBy(x => x.AthleteHistoryLoses)
                            .Skip(from)
                            .Take(limit)
                            .ToListAsync();
        }

        public static async Task<int> GetHonorRankCountAsync()
        {
            await using var ctx = new ServerDbContext();
            return await ctx.Characters
                            .Where(x => x.AthleteHistoryHonorPoints > 0)
                            .OrderByDescending(x => x.AthleteHistoryHonorPoints)
                            .ThenByDescending(x => x.AthleteHistoryWins)
                            .ThenBy(x => x.AthleteHistoryLoses)
                            .CountAsync();
        }

        public static async Task<List<DbCharacter>> GetResetAsync(uint today)
        {
            await using var ctx = new ServerDbContext();
            return ctx.Characters.Where(x => x.DayResetDate != today).ToList();
        }

        public static async Task<List<DbPointAllot>> GetPointAllotAsync()
        {
            await using var db = new ServerDbContext();
            return db.PointAllot.ToList();
        }

        public static async Task<List<DbPigeon>> GetPigeonAsync(uint idUser)
        {
            await using var ctx = new ServerDbContext();
            return await ctx.Pigeons.Where(x => x.UserIdentity == idUser).ToListAsync();
        }

        public static async Task<List<DbPigeonQueue>> GetPigeonQueueAsync(uint idUser)
        {
            await using var ctx = new ServerDbContext();
            return await ctx.PigeonQueues.Where(x => x.UserIdentity == idUser).ToListAsync();
        }

        public static async Task<List<DbTrade>> GetTradesAsync(uint idUser)
        {
            await using var ctx = new ServerDbContext();
            return await ctx.Trade.Where(x => x.UserIdentity == idUser || x.TargetIdentity == idUser).ToListAsync();
        }

        public static async Task<List<DbTradeItem>> GetTradeItemAsync(uint idTrade)
        {
            await using var ctx = new ServerDbContext();
            return await ctx.TradeItem.Where(x => x.TradeIdentity == idTrade).ToListAsync();
        }

        public static async Task<List<DbGameLoginRecord>> GetLoginRcdAsync(uint idUser)
        {
            await using var ctx = new ServerDbContext();
            return await ctx.LoginRcd.Where(x => x.UserIdentity == idUser).ToListAsync();
        }
    }
}
