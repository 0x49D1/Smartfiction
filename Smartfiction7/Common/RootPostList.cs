using System.Collections.Generic;
using Smartfiction.Model;

namespace Common
{
    public class RootPostList
    {
        public string status { get; set; }
        public int count { get; set; }
        public int count_total { get; set; }
        public int pages { get; set; }
        public List<Post> posts { get; set; }
    }
}
