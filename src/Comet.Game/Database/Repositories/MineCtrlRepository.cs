using Comet.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comet.Game.Database.Repositories
{
    public static class MineCtrlRepository
    {
        public static async Task<IList<DbMineCtrl>> GetAsync()
        {
            await using var db = new ServerDbContext();
            return await db.MineRates.ToListAsync();
        }
    }
}
