using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace Comet.Game.Database.Models
{
    [Table("cq_fate_rand")]
    public class DbFateRand
    {
        [Column("fate_no")] public virtual byte FateNo { get; set; }
        [Column("range_no")] public virtual byte RangeNo { get; set; }
        [Column("range_rate")] public virtual uint RangeRate { get; set; }

        public static async Task<IList<DbFateRand>> GetAsync()
        {
            await using var ctx = new ServerDbContext();
            return await ctx.FateRands.ToListAsync();
        }
    }
}
