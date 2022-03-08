using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comet.Web.Data.Entities
{
    [Table("account_ban")]
    public class ApplicationUserBan
    {
        [Key]
        public virtual int Id { get; set; }
        public virtual int Data { get; set; }
        public virtual int AccountId { get; set; }
        public virtual DateTime BanTime { get; set; }
        public virtual DateTime ExpireTime { get; set; }
        public virtual int BannedBy { get; set; }
        public virtual int ReasonId { get; set; }
        public virtual string Reason { get; set; } // description
        public virtual int Flags { get; set; }
        // \/\/\/\/ should not be used :]
        public virtual DateTime? UpdatedAt { get; set; }
        public virtual DateTime? DeletedAt { get; set; }
    }
}
