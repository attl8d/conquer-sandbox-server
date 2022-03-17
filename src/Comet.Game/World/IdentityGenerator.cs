using System.Collections.Concurrent;
using System.Linq;
using Comet.Game.States;

namespace Comet.Game.World
{
    public sealed class IdentityGenerator
    {
        public static IdentityGenerator MapItem = new(Role.MAPITEM_FIRST, Role.MAPITEM_LAST);
        public static IdentityGenerator Monster = new(Role.MONSTERID_FIRST, Role.MONSTERID_LAST);
        public static IdentityGenerator Pet = new(Role.PETID_FIRST, Role.PETID_LAST);
        public static IdentityGenerator Furniture = new(Role.SCENE_NPC_MIN, Role.SCENE_NPC_MAX);
        public static IdentityGenerator Traps = new(Role.MAGICTRAPID_FIRST, Role.MAGICTRAPID_LAST);

        private readonly ConcurrentQueue<long> mIdQueue = new();
        private long mIdNext;

        public IdentityGenerator(long min, long max)
        {
            long idMin;
            mIdNext = idMin = min;
            long idMax = max;

            for (long i = idMin; i <= idMax; i++) mIdQueue.Enqueue(i);

            mIdNext = idMax + 1;
        }

        public long GetNextIdentity
        {
            get
            {
                if (mIdQueue.TryDequeue(out long result))
                    return result;
                return 0;
            }
        }

        public void ReturnIdentity(long id)
        {
            if (!mIdQueue.Contains(id))
                mIdQueue.Enqueue(id);
        }

        public int IdentitiesCount()
        {
            return mIdQueue.Count;
        }
    }
}