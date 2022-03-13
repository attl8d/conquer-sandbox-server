using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comet.Web.Data.Entities
{
    [Table("records_family")]
    public class RecordFamily
    {
        [Key] public uint Identity { get; set; }
        public uint ServerIdentity { get; set; }
        public uint FamilyIdentity { get; set; }
        public string Name { get; set; }
        public uint LeaderIdentity { get; set; }
        public ulong Money { get; set; }
        public uint Count { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public uint ChallengeMap { get; set; }
        public uint DominatedMap { get; set; }
        public byte Level { get; set; }
        public byte BpTower { get; set; }
    }
}