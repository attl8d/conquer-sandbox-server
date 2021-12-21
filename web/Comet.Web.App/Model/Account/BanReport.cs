using System;
using System.Collections.Generic;
using System.Linq;

namespace Comet.Web.App.Model.Account
{
    public class BanReport
    {
        public bool IsAccountBanned => Account.Any(x => x.IsActive);
        public int ActiveBans => Game.Count(x => x.IsActive);

        public List<BanInformation> Account { get; } = new List<BanInformation>();
        public List<BanInformation> Game { get; } = new List<BanInformation>();

        public BanInformation CurrentBan => Account.FirstOrDefault(x => x.IsActive);

        public class BanTerm
        {
            public string Type { get; set; }
            public int Minutes { get; set; }
            public string Terms { get; set; }
            public bool IsPermanent { get; set; }
        }
        
        public class BanInformation
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public BanFlags Flags { get; set; }
            public string BannedBy { get; set; }

            public int ReasonId { get; set; }
            public string Reason { get; set; }
            public string Description { get; set; }

            public DateTime From { get; set; }
            public DateTime To { get; set; }

            public bool IsActive => !IsCancelled && !IsForgiven && !IsFalsePositive && To > DateTime.Now;

            public bool IsCancelled => (Flags & BanFlags.Removed) != 0;
            public bool IsForgiven => (Flags & BanFlags.Forgiven) != 0;
            public bool IsFalsePositive => (Flags & BanFlags.FalsePositive) != 0;

            public int BanIdentity { get; set; }
        }
    }

    [Flags]
    public enum BanFlags
    {
        None = 0x0,
        /// <summary>
        /// This ban has been removed before it's completion time, but it still counts.
        /// </summary>
        Removed = 0x1,
        /// <summary>
        /// This ban has been forgiven and won't count.
        /// </summary>
        Forgiven = 0x2,
        /// <summary>
        /// This ban has been given without a real reason and has been removed.
        /// </summary>
        FalsePositive = 0x4
    }
}
