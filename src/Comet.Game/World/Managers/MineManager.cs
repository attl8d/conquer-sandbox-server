using Comet.Database.Entities;
using Comet.Game.Database.Repositories;
using Comet.Game.States;
using Comet.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Game.World.Managers
{
    public static class MineManager
    {
        private static List<MineCtrl> mMineControl = new List<MineCtrl>();

        public static async Task<bool> InitializeAsync()
        {
            foreach (var ctrl in await MineCtrlRepository.GetAsync())
            {
                DbItemtype it = ItemManager.GetItemtype(ctrl.ItemId);
                if (it == null)
                {
                    await Log.WriteLogAsync(LogLevel.Warning, $"Could not find {ctrl.ItemId} for mining {ctrl.Id}");
                    continue;
                }

                mMineControl.Add(new MineCtrl(ctrl));
            }
            return true;
        }

        public static async Task<uint> MineAsync(uint mapId, Character target)
        {
            var mapPool = mMineControl.Where(x => x.MapId == mapId).OrderBy(x => x.Percent);
            foreach (var ctrl in mapPool)
            {
                if (ctrl.IsPickUpAllowed && await ctrl.TryPickUpAsync())
                {
                    if (target.UserPackage.MultiCheckItem(ctrl.ItemId, ctrl.ItemId, (int)ctrl.Limit)) // user has reached limit
                        continue;

                    ctrl.Refresh();
                    return ctrl.ItemId;
                }
            }
            return 0;
        }
    }

    public class MineCtrl
    {
        private DbMineCtrl mMineCtrl;
        private TimeOut mTimeout;

        public MineCtrl(DbMineCtrl ctrl)
        {
            mMineCtrl = ctrl;
            mTimeout = new TimeOut();
            mTimeout.Startup(ctrl.Interval);
        }

        public uint ItemId => mMineCtrl.ItemId;
        public uint MapId => mMineCtrl.MapId;
        public uint Percent => mMineCtrl.Percent;
        public uint Limit => mMineCtrl.AmountLimit;
        public async Task<bool> TryPickUpAsync() => await Kernel.ChanceCalcAsync((int)mMineCtrl.Percent, 40000000);
        public bool IsPickUpAllowed => mTimeout.IsTimeOut();
        public void Refresh() => mTimeout.Update();
    }
}
