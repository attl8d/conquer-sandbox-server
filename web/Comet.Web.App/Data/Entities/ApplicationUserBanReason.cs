using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comet.Web.App.Data.Entities
{
    [Table("account_ban_reason")]
    public class ApplicationUserBanReason
    {
        [Key]
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual int Type { get; set; }
        public virtual int GameType { get; set; }
    }
}
