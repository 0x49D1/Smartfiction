using System;
using System.Linq;
using System.Net;
using System.Threading;
using BugSense;
using Newtonsoft.Json;

namespace Smartfiction.Model
{
    public class StoryRepository
    {
        private static bool flag = false;
        private static string detailsGlobal = null;

        public static int AddNewStory(string title,
                                        DateTime datePublished,
                                        string link,
                                        string details)
        {
            try
            {
                flag = false;

                if (details.Contains("[...]"))
                {
                    // try to get full content from server
                    var wc = new WebClient();
                    wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webClient_OpenReadCompleted);
                    try
                    {
                        wc.DownloadStringAsync(new Uri(link + "?json=1"));
                    }
                    catch (Exception exception)
                    {
                        BugSenseHandler.Instance.LogException(exception);
                        flag = true;
                    }
                }
                else
                    flag = true;

                while (flag)
                {
                    Thread.Sleep(100);
                }

                using (StoryDataContext context = ConnectionFactory.GetStoryDataContext())
                {
                    if (CheckStoryTitle(title))
                        return 0;
                    Story s = new Story();
                    s.Title = title;
                    s.DateCreated = DateTime.Now;
                    s.DatePublished = datePublished;
                    s.Link = link;
                    s.Details = detailsGlobal ?? details;

                    context.Stories.InsertOnSubmit(s);

                    context.SubmitChanges();
                    return s.StoryID;
                }
            }
            catch (Exception e)
            {
                BugSense.BugSenseHandler.Instance.LogException(e);
            }
            return -1;
        }

        private static void webClient_OpenReadCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            flag = true;
            if (e.Cancelled || e.Result == null)
                return;

            var value = JsonConvert.DeserializeObject<PostRoot>(e.Result);
            detailsGlobal = value.post.content;
        }

        public static bool RemoveStory(Story story)
        {
            try
            {
                using (StoryDataContext context = ConnectionFactory.GetStoryDataContext())
                {
                    context.Stories.Attach(story);
                    context.Stories.DeleteOnSubmit(story);
                    context.SubmitChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                BugSense.BugSenseHandler.Instance.LogException(e);
            }
            return false;
        }

        /// <summary>
        /// Remove story by title
        /// </summary>
        /// <param name="title">Title to remove from stories</param>
        /// <returns>Success/Failure</returns>
        public static bool RemoveStory(string title)
        {
            try
            {
                using (StoryDataContext context = ConnectionFactory.GetStoryDataContext())
                {
                    var story = context.Stories.FirstOrDefault(s => s.Title == title);
                    if (story == null)
                        return true;

                    context.Stories.DeleteOnSubmit(story);
                    context.SubmitChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                BugSense.BugSenseHandler.Instance.LogException(e);
            }
            return false;
        }

        // Check if story with selected link already exists
        public static bool CheckStory(string link)
        {
            using (StoryDataContext context = ConnectionFactory.GetStoryDataContext())
            {
                return context.Stories.Any(s => s.Link == link);
            }
        }

        // Check if story with selected title already exists
        public static bool CheckStoryTitle(string title)
        {
            using (StoryDataContext context = ConnectionFactory.GetStoryDataContext())
            {
                return context.Stories.Any(s => s.Title == title);
            }
        }

        public static Story GetSingleStory(int storyID)
        {
            using (StoryDataContext context = ConnectionFactory.GetStoryDataContext())
            {
                return context.Stories.FirstOrDefault(s => s.StoryID == storyID);
            }
        }

        public static Story GetSingleStoryByTitle(string title)
        {
            using (StoryDataContext context = ConnectionFactory.GetStoryDataContext())
            {
                return context.Stories.FirstOrDefault(s => s.Title == title);
            }
        }
    }
}
