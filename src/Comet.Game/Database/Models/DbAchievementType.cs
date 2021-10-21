using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace Comet.Game.Database.Models
{
    [Table("cq_achievementtype")]
    public class DbAchievementType
    {
        [Key]
        [Column("id")]
        public uint Identity { get; set; }
        [Column("name")] public string Name { get; set; }
        [Column("position")] public ushort Position { get; set; }
        [Column("point")] public byte Point { get; set; }
        [Column("facebook")] public byte Facebook { get; set; }

        public static async Task<List<DbAchievementType>> GetAllAsync()
        {
            await using var db = new ServerDbContext();
            return await db.AchievementTypes.ToListAsync();
        }
    }
}
