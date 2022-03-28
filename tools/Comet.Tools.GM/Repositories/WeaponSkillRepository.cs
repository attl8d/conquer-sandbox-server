using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Comet.Database.Entities;

namespace Comet.Tools.GM.Repositories
{
    internal class WeaponSkillRepository
    {
        public static async Task<List<DbWeaponSkill>> GetAsync(uint idUser)
        {
            await using var db = new ServerDbContext();
            return db.WeaponSkills.Where(x => x.OwnerIdentity == idUser).ToList();
        }
    }
}
