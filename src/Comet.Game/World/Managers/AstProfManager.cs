using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Comet.Game.World.Managers
{
    public sealed class AstProfManager
    {
        private ConcurrentDictionary<uint, object> mDictAstProfs = new ConcurrentDictionary<uint, object>();

        public async Task<bool> InitializeAsync()
        {
            return true;
        }
    }
}
