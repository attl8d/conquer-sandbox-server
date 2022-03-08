using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comet.Web.Data.Entities
{
    [Table("account_vip_vind")]
    public class GameAccountVipBind
    {
        [Key] public virtual long Id { get; protected set; }
        public virtual int UserId { get; set; }
        public virtual int ServerId { get; set; }
        public virtual int? TargetId { get; set; }
        public virtual int? CardId { get; set; }
        public virtual DateTime BindTime { get; set; }
        public virtual DateTime ExpirationTime { get; set; }
        public virtual DateTime? CancellationTime { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual RealmData Server { get; set; }
        public virtual CreditCard Card { get; set; }
    }
}
