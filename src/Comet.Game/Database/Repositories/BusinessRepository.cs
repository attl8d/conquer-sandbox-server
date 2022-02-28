using Comet.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Game.Database.Repositories
{
    public static class BusinessRepository
    {
        public static async Task<List<DbBusiness>> GetAsync(uint sender)
        {
            await using ServerDbContext ctx = new ServerDbContext();
            return await ctx.Business.Where(x => x.UserId == sender || x.BusinessId == sender)
                .Include(x => x.User)
                .Include(x => x.Business)
                .ToListAsync();
        }
    }
}
