using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Comet.Web.Data.Entities
{
    [Table("account_user_roles")]
    public class ApplicationUserRole : IdentityUserRole<int>
    {
        [Key]
        [ForeignKey("FK_AspNetUserRoles_AspNetUsers_UserId")]
        public override int UserId { get; set; }

        [Key]
        [ForeignKey("FK_AspNetUserRoles_AspNetRoles_RoleId")]
        public override int RoleId { get; set; }

        [Column] public DateTime CreationDate { get; set; }
        [Column] public DateTime? ModifiedDate { get; set; }
    }
}