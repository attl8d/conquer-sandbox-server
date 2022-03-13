using System;

namespace Comet.Web.Model.Articles
{
    public class ArticleCommentResultModel : ApiDefaultResultModel
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string CreatorName { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}