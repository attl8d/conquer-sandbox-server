using System.Collections.Concurrent;
using static Comet.Game.Packets.MsgSubPro;

namespace Comet.Game.States
{
    public sealed class SubClass
    {
        private ConcurrentDictionary<AstProfType, AstProf> mDictAstProfs = new ConcurrentDictionary<AstProfType, AstProf>();
        
    }
}
