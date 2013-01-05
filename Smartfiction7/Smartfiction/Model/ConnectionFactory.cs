using System.Data.Linq;

namespace Smartfiction.Model
{
    public static class ConnectionFactory
    {
        private const string strConnectionString = @"isostore:/SmartfictionDB.sdf";

        public static DataContext GetDataContext(string type)
        {
            if (type.ToLower() == "story")
                return new StoryDataContext(strConnectionString);
            return null;
        }

        public static StoryDataContext GetStoryDataContext()
        {
            return new StoryDataContext(strConnectionString);
        }
    }
}
