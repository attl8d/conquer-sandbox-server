using Comet.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comet.Game.Database.Repositories
{
    public static class TrapRepository
    {
        public static async Task<List<DbTrap>> GetAsync()
        {
            await using ServerDbContext ctx = new ServerDbContext();
            return await ctx.Traps
                .Include(x => x.Type)
                .ToListAsync();
        }
    }
}
