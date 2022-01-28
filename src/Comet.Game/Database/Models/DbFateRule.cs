using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace Comet.Game.Database.Models
{
    [Table("cq_fate_rule")]
    public class DbFateRule
    {
        [Key][Column("id")] public virtual uint Id { get; set; }
        [Column("fate_no")] public virtual byte FateNo { get; set; }
        [Column("attib_type")] public virtual byte AttrType { get; set; }
        [Column("appear_weight")] public virtual byte AppearWeight { get; set; }
        [Column("attrib_value_min")] public virtual int AttribValueMin { get; set; }
        [Column("attrib_value_max")] public virtual int AttribValueMax { get; set; }

        public static async Task<IList<DbFateRule>> GetAsync()
        {
            await using var ctx = new ServerDbContext();
            return await ctx.FateRules.ToListAsync();
        }
    }
}