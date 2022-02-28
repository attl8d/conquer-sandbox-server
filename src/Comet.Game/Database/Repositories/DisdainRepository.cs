using Comet.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comet.Game.Database.Repositories
{
    public static class DisdainRepository
    {
        public static async Task<List<DbDisdain>> GetAsync()
        {
            await using var ctx = new ServerDbContext();
            return await ctx.Disdains.ToListAsync();
        }
    }
}
