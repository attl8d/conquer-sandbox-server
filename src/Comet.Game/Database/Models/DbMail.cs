using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Game.Database.Models
{
    [Table("cq_mail")]
    public class DbMail
    {
        [Key]
        [Column("id")] public virtual uint Identity { get; protected set; }
        [Column("receiver_id")] public virtual uint ReceiverId { get; set; }
        [Column("Sender_name")] public virtual string SenderName { get; set; }
        [Column("title")] public virtual string Title { get; set; }
        [Column("content")] public virtual string Content { get; set; }
        [Column("money")] public virtual ulong Money { get; set; }
        [Column("emoney")] public virtual uint ConquerPoints { get; set; }
        [Column("action")] public virtual uint Action { get; set; }
        [Column("expiration_date")] public virtual uint ExpirationDate { get; set; }
        [Column("item_id")] public virtual uint ItemIdentity { get; set; }
        [Column("emoney_record_type")] public virtual byte ConquerPointsRecordType { get; set; }
        [Column("flag")] public virtual byte Flag { get; set; }

        public static async Task<List<DbMail>> GetListAsync(uint idUser)
        {
            await using var db = new ServerDbContext();
            return await db.Mails.Where(x => x.ReceiverId == idUser)
                .ToListAsync();
        }

        public static async Task<DbMail> GetAsync(uint idEmail)
        {
            await using var db = new ServerDbContext();
            return await db.Mails.FirstOrDefaultAsync(x => x.Identity == idEmail);
        }
    }
}
