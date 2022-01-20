using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace Comet.Game.Database.Models
{
    [Table("cq_ast_prof_promote_condition")]
    public class DbAstProfPromoteCondition
    {
        [Key]
        public virtual uint Identity { get; set; }
        public virtual byte AstProfType { get; set; }
        public virtual byte AstProfRank { get; set; }
        public virtual byte Metempsychosis { get; set; }
        public virtual byte UserLevel { get; set; }
        public virtual byte AstProfLevel { get; set; }

        public static async Task<List<DbAstProfPromoteCondition>> GetAsync()
        {
            await using var ctx = new ServerDbContext();
            return await ctx.AstProfPromoteConditions.ToListAsync();
        }
    }
}
