using Comet.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comet.Game.Database.Repositories
{
    public static class PigeonQueueRepository
    {
        public static async Task<List<DbPigeonQueue>> GetAsync()
        {
            await using ServerDbContext ctx = new ServerDbContext();
            return await ctx.PigeonQueues.ToListAsync();
        }
    }
}
