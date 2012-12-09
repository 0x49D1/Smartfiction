using System.Data.Linq;

namespace Smartfiction.Model
{
    public class StoryDataContext : DataContext
    {
        public StoryDataContext(string connectionString)
            : base(connectionString)
        {
        }
        public Table<Story> Stories
        {
            get
            {
                return this.GetTable<Story>();
            }
        }
    }
}
