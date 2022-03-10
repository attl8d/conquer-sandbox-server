using System;

namespace Comet.Web.Areas.Panel.Models
{
    public class AdminRoleListModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int Count { get; set; }
    }
}
