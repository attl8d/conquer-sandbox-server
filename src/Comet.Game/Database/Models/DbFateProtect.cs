using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Game.Database.Models
{
    [Table("cq_fate_protect")]
    public class DbFateProtect
    {
        [Key][Column("id")] public virtual uint Id { get; set; }
        [Column("player_id")] public virtual uint PlayerId { get; set; }
        [Column("fate_no")] public virtual byte FateNo { get; set; }
        [Column("expiry_date")] public virtual int ExpiryDate { get; set; }
        [Column("attrib_no1")] public virtual int Attrib1 { get; set; }
        [Column("attrib_no2")] public virtual int Attrib2 { get; set; }
        [Column("attrib_no3")] public virtual int Attrib3 { get; set; }
        [Column("attrib_no4")] public virtual int Attrib4 { get; set; }

        public static async Task<IList<DbFateProtect>> GetAsync(uint idUser)
        {
            await using var ctx = new ServerDbContext();
            return await ctx.FateProtects.Where(x => x.PlayerId == idUser).ToListAsync();
        }
    }
}
