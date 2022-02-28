using Comet.Database.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Game.Database.Repositories
{
    public static class MonsterypeRepository
    {
        public static async Task<List<DbMonstertype>> GetAsync()
        {
            await using ServerDbContext context = new ServerDbContext();
            return context.Monstertype.ToList();
        }
    }
}
