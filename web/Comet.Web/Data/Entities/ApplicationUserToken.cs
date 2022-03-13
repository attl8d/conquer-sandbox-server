using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Comet.Web.Data.Entities
{
    [Table("account_tokens")]
    public class ApplicationUserToken : IdentityUserToken<int>
    {
        [Key]
        [ForeignKey("FK_AspNetUserTokens_AspNetUsers_UserId")]
        public override int UserId
        {
            get => base.UserId;
            set => base.UserId = value;
        }

        [Key]
        public override string LoginProvider
        {
            get => base.LoginProvider;
            set => base.LoginProvider = value;
        }

        [Key]
        public override string Name
        {
            get => base.Name;
            set => base.Name = value;
        }
    }
}