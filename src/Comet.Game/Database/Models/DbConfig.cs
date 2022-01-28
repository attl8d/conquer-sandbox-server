using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Game.Database.Models
{
    [Table("cq_config")]
    public class DbConfig
    {
        [Key] [Column("id")] public virtual uint Id { get; set; }
        [Column("type")] public virtual int Type { get; set; }
        [Column("data1")] public virtual int Data1 { get; set; }
        [Column("data2")] public virtual int Data2 { get; set; }
        [Column("data3")] public virtual int Data3 { get; set; }
        [Column("data4")] public virtual int Data4 { get; set; }
        [Column("data5")] public virtual int Data5 { get; set; }
        [Column("describ")] public virtual string Description { get; set; }

        public static async Task<IList<DbConfig>> GetAsync(int type)
        {
            await using var ctx = new ServerDbContext();
            return await ctx.Configs.Where(x => x.Type == type).ToListAsync();
        }
    }
}
