using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comet.Web.Data.Entities
{
    [Table("account_ban_reason_factor")]
    public class ApplicationUserBanReasonFactor
    {
        [Key]
        public int ReasonId { get; set; }
        public int Ocurrences { get; set; }
        public int Minutes { get; set; }
        public bool Permanent { get; set; }
        public bool AllowCancel { get; set; }
        public bool CanBeForgiven { get; set; }
    }
}
