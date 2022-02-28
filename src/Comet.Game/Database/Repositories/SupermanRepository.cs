using Comet.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comet.Game.Database.Repositories
{
    internal class SupermanRepository
    {
        public static async Task<List<DbSuperman>> GetAsync()
        {
            await using ServerDbContext ctx = new ServerDbContext();
            return await ctx.Superman.ToListAsync();
        }
    }
}
