using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comet.Web.Data.Entities
{
    [Table("records_user")]
    public class RecordUser
    {
        [Key] public virtual uint Id { get; set; }
        public virtual uint ServerIdentity { get; set; }
        public virtual uint UserIdentity { get; set; }
        public virtual uint AccountIdentity { get; set; }
        public virtual string Name { get; set; }
        public virtual uint MateId { get; set; }
        public virtual byte Level { get; set; }
        public virtual ulong Experience { get; set; }
        public virtual byte Profession { get; set; }
        public virtual byte OldProfession { get; set; }
        public virtual byte NewProfession { get; set; }
        public virtual byte Metempsychosis { get; set; }
        public virtual ushort Strength { get; set; }
        public virtual ushort Agility { get; set; }
        public virtual ushort Vitality { get; set; }
        public virtual ushort Spirit { get; set; }
        public virtual ushort AdditionalPoints { get; set; }
        public virtual uint SyndicateIdentity { get; set; }
        public virtual ushort SyndicatePosition { get; set; }
        public virtual ulong NobilityDonation { get; set; }
        public virtual byte NobilityRank { get; set; }
        public virtual uint SupermanCount { get; set; }
        public virtual DateTime? DeletedAt { get; set; }
        public virtual ulong Money { get; set; }
        public virtual uint WarehouseMoney { get; set; }
        public virtual uint ConquerPoints { get; set; }
        public virtual uint ConquerPointsMono { get; set; }
        public virtual uint FamilyIdentity { get; set; }
        public virtual ushort FamilyRank { get; set; }

        public virtual uint QualifierPoints { get; set; }
        public virtual uint QualifierTotalWins { get; set; }
        public virtual uint QualifierTotalLoses { get; set; }
        public virtual uint QualifierTodayWins { get; set; }
        public virtual uint QualifierTodayLoses { get; set; }
        public virtual uint CurrentHonorPoint { get; set; }
        public virtual uint HistoryHonorPoint { get; set; }
        public virtual uint RedRose { get; set; }
        public virtual uint WhiteRose { get; set; }
        public virtual uint Orchid { get; set; }
        public virtual uint Tulip { get; set; }
        public virtual bool Deleted { get; set; }
    }
}