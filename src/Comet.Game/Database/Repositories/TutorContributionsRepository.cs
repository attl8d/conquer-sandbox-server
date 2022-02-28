using Comet.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Game.Database.Repositories
{
    public static class TutorContributionRepository
    {
        public static async Task<List<DbTutorContribution>> GetStudentsAsync(uint idGuide)
        {
            await using ServerDbContext ctx = new ServerDbContext();
            return await ctx.TutorContributions
                .Where(x => x.TutorIdentity == idGuide)
                .ToListAsync();
        }

        public static async Task<DbTutorContribution> GetGuideAsync(uint idStudent)
        {
            await using ServerDbContext ctx = new ServerDbContext();
            return await ctx.TutorContributions
                .FirstOrDefaultAsync(x => x.StudentIdentity == idStudent);
        }
    }
}
