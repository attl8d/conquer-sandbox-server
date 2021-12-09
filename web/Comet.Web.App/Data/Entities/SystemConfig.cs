using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comet.Web.App.Data.Entities
{
    [Table("system_config")]
    public class SystemConfig
    {
        [Key]
        public virtual string Key { get; set; }
        public virtual string StrData0 { get; set; }
        public virtual string StrData1 { get; set; }
        public virtual string StrData2 { get; set; }
        public virtual string StrData3 { get; set; }
        public virtual string Text { get; set; }
        public virtual long Data0 { get; set; }
        public virtual long Data1 { get; set; }
        public virtual long Data2 { get; set; }
        public virtual long Data3 { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual DateTime? ModifiedDate { get; set; }
        public virtual DateTime? DeleteDate { get; set; }

        public enum ConfigKey
        {
            MaintenanceSettings,
        }
    }
}
