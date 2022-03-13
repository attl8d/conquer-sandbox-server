using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comet.Web.Data.Entities
{
    [Table("article_comment")]
    public class ArticleComment
    {
        [Key] public virtual int Id { get; set; }

        public virtual int ArticleId { get; set; }
        public virtual int UserId { get; set; }
        public virtual string TokenId { get; set; }
        public virtual string IpAddress { get; set; }
        public virtual string Message { get; set; }
        public virtual int Flag { get; set; }
        public virtual int Likes { get; set; }
        public virtual int Dislikes { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual DateTime? UpdateDate { get; set; }
        public virtual DateTime? DeleteDate { get; set; }

        public virtual Article Article { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}