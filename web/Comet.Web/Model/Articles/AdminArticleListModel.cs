using System.Collections.Generic;

namespace Comet.Web.Model.Articles
{
    public class AdminArticleListModel
    {
        public int Id { get; set; }
        public string Writer { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string PublishDate { get; set; }
        public List<string> Locales { get; set; } = new List<string>();
    }
}
