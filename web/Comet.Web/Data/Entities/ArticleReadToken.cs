using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comet.Web.Data.Entities
{
    [Table("article_read_token")]
    public class ArticleReadToken
    {
        [Key]
        public virtual long Id { get; set; }
        public uint ArticleId { get; set; }
        public string Referer { get; set; }
        public string TokenId { get; set; }
        public int? UserId { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public int CreationDate { get; set; }
    }
}