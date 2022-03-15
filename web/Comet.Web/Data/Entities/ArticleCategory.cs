using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comet.Web.Data.Entities
{
    [Table("article_category")]
    public class ArticleCategory
    {
        [Key] public virtual int Id { get; set; }

        public virtual string Name { get; set; }

        [ForeignKey("FK_Article_Category_Type")]
        public virtual int CategoryTypeId { get; set; }
        public virtual int Order { get; set; }
        public virtual int UserId { get; set; }
        public virtual int Flags { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual DateTime? ModifiedDate { get; set; }
        public virtual DateTime? DeletedDate { get; set; }

        public virtual ArticleCategoryType CategoryType { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual IList<Article> Articles { get; set; }
    }
}