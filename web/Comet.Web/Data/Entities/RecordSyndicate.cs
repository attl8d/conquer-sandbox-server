using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comet.Web.Data.Entities
{
    [Table("records_syndicate")]
    public class RecordSyndicate
    {
        [Key] public uint Id { get; set; }
        public uint ServerIdentity { get; set; }
        public uint SyndicateIdentity { get; set; }
        public string Name { get; set; }
        public uint LeaderIdentity { get; set; }
        public long Money { get; set; }
        public uint Count { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
