using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comet.Web.Data.Entities
{
    [Table("log_history")]
    public class LogHistory
    {
        [Key] public virtual int Id { get; set; }

        public virtual int? AccountId { get; set; }
        public virtual LogType Type { get; set; }
        public virtual long Data { get; set; }
        public virtual string Message { get; set; }
        public virtual string Payload { get; set; }

        public enum LogType
        {
            None,
            AccountBan,
            CancelBan,
            ForgiveBan,
            FalsePositiveBan,
            ViewAdminIndexPage,
            ViewAdminUsersPage,
            ViewAdminUserPage,
            ViewAdminRolesPage,
            ViewAdminRolePage,
            ViewAdminBansPage,
            ViewAdminBanDetailPage,
            ChangeAccountPassword,
            ChangeGamePassword,
            LogIn,
            LogOut,
            GameAccountCreation,
            DailyUpdate
        }
    }
}