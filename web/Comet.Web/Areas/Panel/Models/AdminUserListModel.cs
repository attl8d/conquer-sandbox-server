using System;

namespace Comet.Web.Areas.Panel.Models
{
    public class AdminUserListModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime CreationDate { get; set; }
    }
}