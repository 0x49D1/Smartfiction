using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BugSense;
using Newtonsoft.Json;

namespace Smartfiction.Model
{
    public class StoryRepository
    {
        private static bool flag = false;
        private static string detailsGlobal = null;
        public static int InsertedID = 0;

        public static int AddNewStory(string title,
                                        DateTime datePublished,
                                        string link,
                                        string details, bool isFavorite = false, Action<int> callback = null)
        {
            try
            {
                InsertedID = 0;
                if (!Utilities.CheckNetwork())
                    return 0;
                // try to get full content from server
                var wc = new WebClient();

                wc.DownloadStringCompleted += (s, e) =>
                                                  {
                                                      if (e.Cancelled || e.Result == null)
                                                          return;

                                                      var value = JsonConvert.DeserializeObject<PostRoot>(e.Result);
                                                      detailsGlobal = value.post.content;

                                                      using (StoryDataContext context = ConnectionFactory.GetStoryDataContext())
                                                      {
                                                          if (!isFavorite)
                                                          {
                                                              if (CheckStoryTitle(value.post.title))
                                                                  return;
                                                          }
                                                          else
                                                          {
                                                              if (CheckStoryTitle(value.post.title, true))
                                                              {
                                                                  var favstory = context.Stories.FirstOrDefault(sa => sa.Title == value.post.title); ;
                                                                  favstory.IsFavorite = true;
                                                                  context.SubmitChanges();
                                                                  if (callback != null)
                                                                      callback(favstory.StoryID);
                                                                  return;
                                                              }
                                                          }
                                                          Story st = new Story();
                                                          st.Title = value.post.title;
                                                          st.DateCreated = DateTime.Now;
                                                          st.DatePublished = DateTime.Parse(value.post.date);
                                                          st.Link = value.post.url;
                                                          st.Details = detailsGlobal;
                                                          st.IsFavorite = isFavorite;

                                                          context.Stories.InsertOnSubmit(st);

                                                          try
                                                          {
                                                              context.SubmitChanges();
                                                          }
                                                          catch (Exception exception)
                                                          {

                                                          }
                                                          if (callback != null)
                                                              callback(st.StoryID);
                                                      }
                                                  };
                try
                {
                    wc.DownloadStringAsync(new Uri(link + "?json=1"));
                    return 1;
                }
                catch (Exception exception)
                {
                    BugSenseHandler.Instance.LogException(exception);
                }


                //while (!flag)
                //{
                //    Thread.Sleep(100);
                //}

                //using (StoryDataContext context = ConnectionFactory.GetStoryDataContext())
                //{
                //    if (CheckStoryTitle(title))
                //        return 0;
                //    Story s = new Story();
                //    s.Title = title;
                //    s.DateCreated = DateTime.Now;
                //    s.DatePublished = datePublished;
                //    s.Link = link;
                //    s.Details = detailsGlobal ?? details;

                //    context.Stories.InsertOnSubmit(s);

                //    context.SubmitChanges();
                //    return s.StoryID;
                //}
            }
            catch (Exception e)
            {
                BugSense.BugSenseHandler.Instance.LogException(e);
            }
            return -1;
        }

        public static bool RemoveStory(Story story)
        {
            try
            {
                using (StoryDataContext context = ConnectionFactory.GetStoryDataContext())
                {
                    var fav = context.Stories.FirstOrDefault(s => s.Title == story.Title);
                    if (fav == null)
                        return true;

                    //context.Stories.Attach(story);
                    context.Stories.DeleteOnSubmit(fav);
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
        /// <param name="fromFavorits">Remove story completely or just from favorits</param>
        /// <returns>Success/Failure</returns>
        public static bool RemoveStory(string title, bool fromFavorits = false)
        {
            try
            {
                using (StoryDataContext context = ConnectionFactory.GetStoryDataContext())
                {
                    var story = context.Stories.FirstOrDefault(s => s.Title == title);
                    if (story == null)
                        return true;

                    if (!fromFavorits)
                        context.Stories.DeleteOnSubmit(story);
                    else
                        story.IsFavorite = false;
                    context.Stories.Attach(story);
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
        public static bool CheckStoryTitle(string title, bool isFavorit = false)
        {
            using (StoryDataContext context = ConnectionFactory.GetStoryDataContext())
            {
                return context.Stories.Any(s => s.Title == title && s.IsFavorite == isFavorit);
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
