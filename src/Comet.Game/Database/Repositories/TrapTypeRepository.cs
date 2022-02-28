using Comet.Database.Entities;
using System.Threading.Tasks;

namespace Comet.Game.Database.Repositories
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
