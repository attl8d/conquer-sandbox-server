using Comet.Database.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Game.Database.Repositories
{
    public static class PeerageRepository
    {
        public static async Task<List<DbPeerage>> GetAsync()
        {
            await using ServerDbContext context = new ServerDbContext();
            return context.Peerage.ToList();
        }
    }
}
