using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace Comet.Game.Database.Models
{
    [Table("cq_fate_rank")]
    public class DbFateRank
    {
        [Key][Column("id")] public virtual uint Id { get; set; }
        [Column("fate_no")] public virtual byte FateNo { get; set; }
        [Column("sort")] public virtual byte Sort { get; set; }
        [Column("attrib1")] public virtual int Attrib1 { get; set; }
        [Column("attrib2")] public virtual int Attrib2 { get; set; }
        [Column("attrib3")] public virtual int Attrib3 { get; set; }
        [Column("attrib4")] public virtual int Attrib4 { get; set; }

        public static async Task<IList<DbFateRank>> GetAsync()
        {
            await using var ctx = new ServerDbContext();
            return await ctx.FateRanks.ToListAsync();
        }
    }
}
