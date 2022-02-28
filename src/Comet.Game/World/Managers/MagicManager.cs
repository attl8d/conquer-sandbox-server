using Comet.Database.Entities;
using Comet.Game.Database.Repositories;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Game.World.Managers
{
    public static class MagicManager
    {
        private static ConcurrentDictionary<uint, DbMagictype> m_magicType = new ConcurrentDictionary<uint, DbMagictype>();

        public static async Task InitializeAsync()
        {
            foreach (var magicType in await MagictypeRepository.GetAsync())
            {
                m_magicType.TryAdd(magicType.Id, magicType);
            }
        }

        public static byte GetMaxLevel(uint idType)
        {
            return (byte)(m_magicType.Values.Where(x => x.Type == idType).OrderByDescending(x => x.Level).FirstOrDefault()?.Level ?? 0);
        }

        public static DbMagictype GetMagictype(uint idType, ushort level)
        {
            return m_magicType.Values.FirstOrDefault(x => x.Type == idType && x.Level == level);
        }
    }
}
