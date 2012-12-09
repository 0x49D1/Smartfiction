using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartfiction.Model
{
    [Table]
    public class Story
    {
        [Column(IsPrimaryKey = true)]
        public int StoryID { get; set; }

        [Column(CanBeNull = false)]
        public string Title { get; set; }

        [Column(CanBeNull = false)]
        public DateTime DatePublished { get; set; }

        [Column(CanBeNull = false)]
        public string Link { get; set; }
        
        [Column(CanBeNull = false)]
        public DateTime DateCreated { get; set; }
    }
}
