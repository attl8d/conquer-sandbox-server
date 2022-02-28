using Comet.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Game.Database.Repositories
{
    public static class RegionRepository
    {
        public static async Task<List<DbRegion>> GetAsync(uint idMap)
        {
            await using ServerDbContext ctx = new ServerDbContext();
            return await ctx.Regions.Where(x => x.MapIdentity == idMap).ToListAsync();
        }
    }
}
