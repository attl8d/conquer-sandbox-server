using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comet.Web.Data.Entities
{
    [Table("article_category_type")]
    public class ArticleCategoryType
    {
        [Key]
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual int UserId { get; set; }
        public virtual int Flags { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual DateTime? ModifiedDate { get; set; }
        public virtual DateTime? DeletedDate { get; set; }
    }
}
