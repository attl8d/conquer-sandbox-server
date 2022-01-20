using Comet.Game.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comet.Game.World.Managers
{
    public static class AstProfManager
    {
        private static List<DbAstProfInaugurationCondition> mDictAstProfInit = new List<DbAstProfInaugurationCondition>();
        private static List<DbAstProfPromoteCondition> mDictAstProfPromote = new List<DbAstProfPromoteCondition> ();

        public static async Task<bool> InitializeAsync()
        {

            return true;
        }
    }
}
