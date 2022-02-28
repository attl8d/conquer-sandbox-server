using Comet.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Comet.Game.Database.Repositories
{
    public static class TutorAccessRepository
    {
        public static async Task<DbTutorAccess> GetAsync(uint idGuide)
        {
            await using ServerDbContext ctx = new ServerDbContext();
            return await ctx.TutorAccess.FirstOrDefaultAsync(x => x.GuideIdentity == idGuide);
        }
    }
}
