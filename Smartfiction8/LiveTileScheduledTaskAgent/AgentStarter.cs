using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Phone.Scheduler;

namespace LiveTileScheduledTaskAgent
{
    /// <summary>
    /// Updates live tile with story of the day
    /// </summary>
    public static class AgentStarter
    {
        private static string periodicTaskName = "SmartfictionTileUpdater";
        private static PeriodicTask periodicTask;
        private static string previousTaskDescription = "";
        private static DateTime? lastCheckTime = null;

        public static void StartPeriodicAgent()
        {
            // is old task running, remove it
            periodicTask = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;
            if (periodicTask != null)
            {
                try
                {
                    previousTaskDescription = periodicTask.Description;
                    ScheduledActionService.Remove(periodicTaskName);
                }
                catch (Exception)
                {
                }
            }
            lastCheckTime = RetrieveCheckTime();
            // create a new task
            periodicTask = new PeriodicTask(periodicTaskName);
            // Adding this condition to run task once a day
            if (lastCheckTime == null || lastCheckTime - DateTime.Now < TimeSpan.FromHours(13))
            {
                    WebClient client = new WebClient();

                    client.Encoding = System.Text.Encoding.UTF8;

                    client.DownloadStringCompleted +=
                        new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
                    client.DownloadStringAsync(new Uri("http://smartfiction.ru/feed"));
                }
            else
            {
                client_DownloadStringCompleted(null, null);
            }
        }

        private static void StoreLastCheckTime()
        {
            // get user's store
            var storage = IsolatedStorageFile.GetUserStoreForApplication();

            if (storage.DirectoryExists("SmartfictionStorage") == false)
            {
                // if directory does not exist, create it
                storage.CreateDirectory("SmartfictionStorage");
            }

            if (storage.FileExists("checktime.xml"))
            {
                // if file already exists, delete it to reset
                storage.DeleteFile("checktime.xml");
            }

            using (var storageFile = storage.CreateFile("SmartfictionStorage\\checktime.xml"))
            {
                // create the file and serialize the value
                var xmlSerializer = new XmlSerializer(typeof(string));
                xmlSerializer.Serialize(storageFile, DateTime.Now.ToString());
            }
        }

        private static DateTime? RetrieveCheckTime()
        {
            // get the user's store
            var storage = IsolatedStorageFile.GetUserStoreForApplication();

            if (storage.DirectoryExists("SmartfictionStorage")
                    && storage.FileExists("SmartfictionStorage\\checktime.xml"))
            {
                // if file exists in directory, open the file to read
                using (var storageFile =
                    storage.OpenFile("SmartfictionStorage\\checktime.xml", FileMode.Open))
                {
                    var xmlSerializer = new XmlSerializer(typeof(string));

                    // deserialize and return the value
                    DateTime date = DateTime.MinValue;
                    try
                    {
                        if (DateTime.TryParse(xmlSerializer.Deserialize(storageFile).ToString(), out date))
                            return date;
                    }
                    catch (Exception e)
                    {

                    }
                }
            }

            return null; // return default value, for the first time
        }

        private static void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e != null && !string.IsNullOrEmpty(e.Result) &&
                (lastCheckTime == null || lastCheckTime - DateTime.Now < TimeSpan.FromDays(-1)))
            {
                XmlReader reader = XmlReader.Create(new StringReader(e.Result));
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                foreach (SyndicationItem sItem in feed.Items)
                {
                    periodicTask.Description = sItem.Title.Text +
                                               " is story of the day, and this updater will update your story of the day tile.";
                    StoreLastCheckTime();
                    break;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(previousTaskDescription))
                    previousTaskDescription = "Updater will update your story of the day tile.";
                periodicTask.Description = previousTaskDescription;
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
