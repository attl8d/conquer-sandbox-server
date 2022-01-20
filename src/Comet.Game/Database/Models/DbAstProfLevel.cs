using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Game.Database.Models
{
    [Table("cq_ast_prof_level")]
    public class DbAstProfLevel
    {
        [Key]
        [Column("id")]
        public virtual uint Identity { get; set; }
        [Column("user_id")]
        public virtual uint UserIdentity { get; set; }
        [Column("ast_prof")]
        public virtual byte AstProf { get; set; }
        [Column("level")]
        public virtual byte Level { get; set; }

        public static async Task<List<DbAstProfLevel>> GetAsync(uint idUser)
        {
            await using var ctx = new ServerDbContext();
            return await ctx.AstProfLevels.Where(x => x.UserIdentity == idUser).ToListAsync();
        }
    }
}
