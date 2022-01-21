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
        [Column("id")]
        public virtual uint Identity { get; set; }
        [Column("ast_prof_type")]
        public virtual byte AstProfType { get; set; }
        [Column("ast_prof_rank")]
        public virtual byte AstProfRank { get; set; }
        [Column("metempsychosis")]
        public virtual byte Metempsychosis { get; set; }
        [Column("user_level")]
        public virtual byte UserLevel { get; set; }
        [Column("ast_prof_level")]
        public virtual byte AstProfLevel { get; set; }

        public static async Task<List<DbAstProfPromoteCondition>> GetAsync()
        {
            await using var ctx = new ServerDbContext();
            return await ctx.AstProfPromoteConditions.ToListAsync();
        }
    }
}
