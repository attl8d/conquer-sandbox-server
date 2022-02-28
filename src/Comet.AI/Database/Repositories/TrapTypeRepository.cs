using Comet.Database.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.AI.Database.Repositories
{
    public static class TrapTypeRepository
    {
        public static async Task<DbTrapType> GetAsync(uint id)
        {
            await using ServerDbContext ctx = new ServerDbContext();
            return await ctx.TrapTypes.FindAsync(id);
        }
    }
}
