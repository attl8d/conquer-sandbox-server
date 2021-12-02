using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comet.Web.App.Data.Entities
{
    [Table("article_content")]
    public class ArticleContent
    {
        [Key]
        public virtual int Id { get; set; }
        [ForeignKey("FK_Article_Identifier")]
        public virtual int ArticleId { get; set; }
        [ForeignKey("FK_Article_Initial_Creator")]
        public virtual int WriterId { get; set; }
        public virtual string Locale { get; set; }
        public virtual string Title { get; set; }
        public virtual string Content { get; set; }
        [ForeignKey("FK_Article_Last_Editor")]
        public virtual int? LastEditorId { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual DateTime? ModifiedDate { get; set; }
        public virtual DateTime? DeletedDate { get; set; }

        public virtual Article Article { get; set; }
        public virtual ApplicationUser Writer { get; set; }
        public virtual ApplicationUser LastEditor { get; set; }
    }
}
