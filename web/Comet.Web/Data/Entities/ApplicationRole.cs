using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Comet.Web.Data.Entities
{
    [Table("account_roles")]
    public class ApplicationRole : IdentityRole<int>
    {
        [Key]
        public override int Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        public DateTime CreationDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}