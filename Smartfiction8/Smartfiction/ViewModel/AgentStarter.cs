using System;
using System.IO;
using System.Net;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Xml;
using Microsoft.Phone.Scheduler;

namespace Smartfiction.ViewModel
{
    /// <summary>
    /// Updates live tile with story of the day
    /// </summary>
    public class AgentStarter
    {
        private static string periodicTaskName = "SmartfictionTileUpdater";
        private static PeriodicTask periodicTask;

        public static void StartPeriodicAgent()
        {
            // is old task running, remove it
            periodicTask = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;
            if (periodicTask != null)
            {
                try
                {
                    ScheduledActionService.Remove(periodicTaskName);
                }
                catch (Exception)
                {
                }
            }
            // create a new task
            periodicTask = new PeriodicTask(periodicTaskName);

            foreach (string item in App.Data.FeedList)
            {
                WebClient client = new WebClient();

                client.Encoding = System.Text.Encoding.UTF8;

                client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
                client.DownloadStringAsync(new Uri(item));
            }
        }

        private static void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Result))
            {
                XmlReader reader = XmlReader.Create(new StringReader(e.Result));
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                foreach (SyndicationItem sItem in feed.Items)
                {
                    periodicTask.Description = sItem.Title.Text + " is story of the day, and this updater will update your story of the day tile.";
                    break;
                }

                // set expiration days
                periodicTask.ExpirationTime = DateTime.Now.AddDays(10);
                try
                {
                    // add thas to scheduled action service
                    ScheduledActionService.Add(periodicTask);
                    // debug, so run in every 30 secs
#if DEBUG_AGENT
                    ScheduledActionService.LaunchForTest(periodicTaskName, TimeSpan.FromSeconds(10));
                    System.Diagnostics.Debug.WriteLine("Periodic task is started: " + periodicTaskName);
#endif

                }
                catch (InvalidOperationException exception)
                {
                    if (exception.Message.Contains("BNS Error: The action is disabled"))
                    {
                        // load error text from localized strings
                        MessageBox.Show("Background agents for this application have been disabled by the user.");
                    }
                    if (exception.Message.Contains("BNS Error: The maximum number of ScheduledActions of this type have already been added."))
                    {
                        // No user action required. The system prompts the user when the hard limit of periodic tasks has been reached.
                    }
                }
                catch (SchedulerServiceException)
                {
                    // No user action required.
                }
            }
        }
    }
}
