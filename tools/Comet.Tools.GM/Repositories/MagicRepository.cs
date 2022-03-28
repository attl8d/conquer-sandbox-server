using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Comet.Database.Entities;

namespace Comet.Tools.GM.Repositories
{
    internal class MagicRepository
    {
        public static async Task<List<DbMagic>> GetAsync(uint idUser)
        {
            await using var db = new ServerDbContext();
            return db.Magic.Where(x => x.OwnerId == idUser).ToList();
        }

        public static async Task<List<DbMagictype>> GetMagicTypeAsync()
        {
            await using var db = new ServerDbContext();
            return db.Magictype.ToList();
        }
    }
}
