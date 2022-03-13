using System;

namespace Comet.Web.Areas.Panel.Models
{
    public class AdminBanListModel
    {
        public string BanType { get; set; }
        public int Id { get; set; }
        public string UserName { get; set; }
        public string BannedBy { get; set; }
        public string Reason { get; set; }
        public int Flags { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}