using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comet.Web.Data.Entities
{
    [Table("article")]
    public class Article
    {
        [Key] public virtual int Id { get; set; }

        [ForeignKey("FK_Article_Category")] public virtual int CategoryId { get; set; }

        [ForeignKey("FK_Article_Creator_Identifier")]
        public virtual int UserId { get; set; }

        public virtual ArticleFlags Flags { get; set; }
        public virtual int? ThumbId { get; set; }
        public virtual int? BackgroundId { get; set; }
        public virtual int Views { get; set; }
        public virtual int Order { get; set; }
        public virtual DateTime PublishDate { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual DateTime? ModifiedDate { get; set; }
        public virtual DateTime? DeletedDate { get; set; }

        public virtual ArticleCategory Category { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual IList<ArticleContent> Content { get; set; }
        
        public virtual Images Thumb { get; set; }
        public virtual Images Background { get; set; }

        [Flags]
        public enum ArticleFlags : ulong
        {
            Visible = 0x0,
            NotPublished = 0x1,
            NotListed = 0x2
        }
    }
}