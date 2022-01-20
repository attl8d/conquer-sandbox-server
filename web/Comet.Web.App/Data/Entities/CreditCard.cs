using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comet.Web.App.Data.Entities
{
    [Table("credit_card")]
    public class CreditCard
    {
        [Key] public virtual int Id { get; protected set; }
        public virtual int Type { get; set; }
        public virtual int? UserId { get; set; }
        public virtual long? CheckoutId { get; set; }
        public virtual ushort Part1 { get; set; }
        public virtual ushort Part2 { get; set; }
        public virtual ushort Part3 { get; set; }
        public virtual ushort Part4 { get; set; }
        public virtual string Password { get; set; }
        public virtual DateTime GeneratedAt { get; protected set; }
        public virtual DateTime? UsedAt { get; set; }
        public virtual DateTime? CanceledAt { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
