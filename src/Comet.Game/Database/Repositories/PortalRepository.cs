using Comet.Database.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Game.Database.Repositories
{
    public static class PortalRepository
    {
        public static async Task<DbPortal> GetAsync(uint idMap, uint idx)
        {
            await using ServerDbContext context = new ServerDbContext();
            return context.Portal.FirstOrDefault(x => x.MapId == idMap && x.PortalIndex == idx);
        }
    }
}
