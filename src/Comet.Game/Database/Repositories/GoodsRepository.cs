using Comet.Database.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Game.Database.Repositories
{
    public static class GoodsRepository
    {
        public static async Task<List<DbGoods>> GetAsync(uint idNpc)
        {
            await using ServerDbContext context = new ServerDbContext();
            return context.Goods.Where(x => x.OwnerIdentity == idNpc).ToList();
        }
    }
}
