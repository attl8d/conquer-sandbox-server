using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comet.Web.Data.Entities
{
    [Table("realm")]
    public class RealmData
    {
        [Key]
        public virtual uint RealmID { get; set; }
        public virtual string Name { get; set; }
        public virtual ushort AuthorityID { get; set; }
        public virtual string GameIPAddress { get; set; }
        public virtual string RpcIPAddress { get; set; }
        public virtual uint GamePort { get; set; }
        public virtual uint RpcPort { get; set; }
        public virtual byte Status { get; set; }
        public virtual string Username { get; set; }
        public virtual string Password { get; set; }
        public virtual DateTime? LastPing { get; set; }
        public virtual string DatabaseHost { get; set; }
        public virtual string DatabaseUser { get; set; }
        public virtual string DatabasePass { get; set; }
        public virtual string DatabaseSchema { get; set; }
        public virtual string DatabasePort { get; set; }
        public virtual bool Active { get; set; }
    }
}
