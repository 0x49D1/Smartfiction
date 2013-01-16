using System.Collections.Generic;

namespace Smartfiction.Model
{
    public class Post
    {
        public int id { get; set; }
        public string type { get; set; }
        public string slug { get; set; }
        public string url { get; set; }
        public string status { get; set; }
        public string title { get; set; }
        public string title_plain { get; set; }
        public string content { get; set; }
        public string excerpt { get; set; }
        public string date { get; set; }
        public string modified { get; set; }
        public List<Category> categories { get; set; }
        public List<Tag> tags { get; set; }
        public Author author { get; set; }
        public List<object> comments { get; set; }
        public List<object> attachments { get; set; }
        public int comment_count { get; set; }
        public string comment_status { get; set; }
    }
}