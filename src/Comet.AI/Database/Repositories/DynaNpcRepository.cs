using Comet.Database.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.AI.Database.Repositories
{
    public static class DynaNpcRespository
    {
        public static async Task<List<DbDynanpc>> GetAsync()
        {
            await using ServerDbContext context = new ServerDbContext();
            return context.DynaNpcs.ToList();
        }
    }
}
