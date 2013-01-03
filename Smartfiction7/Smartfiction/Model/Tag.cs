namespace Smartfiction.Model
{
    public class Tag
    {
        public int id { get; set; }
        public string slug { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int post_count { get; set; }
    }
}