using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Comet.Web.Data.Entities
{
    [Table("account_logins")]
    public class ApplicationUserLogin : IdentityUserLogin<int>
    {
        [Key]
        public override string LoginProvider
        {
            get => base.LoginProvider;
            set => base.LoginProvider = value;
        }

        [Key]
        public override string ProviderKey
        {
            get => base.ProviderKey;
            set => base.ProviderKey = value;
        }

        [ForeignKey("FK_AspNetUserLogins_AspNetUsers_UserId")]
        public override int UserId { get; set; }
    }
}