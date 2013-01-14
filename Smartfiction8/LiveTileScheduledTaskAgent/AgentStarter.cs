using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Xml.Serialization;
using Common;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Smartfiction.Model;

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
            ZeroLastCheckTime();
            lastCheckTime = RetrieveCheckTime();
            // create a new task
            periodicTask = new PeriodicTask(periodicTaskName);
            // set expiration days
            periodicTask.ExpirationTime = DateTime.Now.AddDays(10);
            CheckTileTextUpdate();
        }

        public static void CheckTileTextUpdate()
        {
            // Adding this condition to run task once a day
            if (lastCheckTime == null || DateTime.Now - lastCheckTime > TimeSpan.FromHours(6))
            {
                WebClient client = new WebClient();

                client.Encoding = System.Text.Encoding.UTF8;

                client.DownloadStringCompleted +=
                    new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
                client.DownloadStringAsync(new Uri("http://smartfiction.ru/" + "?json=get_recent_posts&count=1"));
            }
            else
            {
                client_DownloadStringCompleted(null, null);
            }
        }

        public static void ZeroLastCheckTime()
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
            if (e == null || e.Cancelled)
                return;

            if (e != null && !string.IsNullOrEmpty(e.Result) &&
                (lastCheckTime == null || DateTime.Now - lastCheckTime > TimeSpan.FromHours(6)))
            {

                ShellTile PrimaryTile = ShellTile.ActiveTiles.First();
                if (PrimaryTile != null)
                {
                    var value = JsonConvert.DeserializeObject<RootPostList>(e.Result);
                    if (value.posts.Count > 0)
                    {
                        StandardTileData tile = new StandardTileData();

                        tile.BackContent = value.posts[0].title;
                        // to make tile flip add data to background also
                        tile.BackTitle = "";
                        // For white theme show white square
                        tile.BackBackgroundImage =
                            //((Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible)
                            new Uri("/Images/black.png", UriKind.Relative);
                        PrimaryTile.Update(tile);
                        StoreLastCheckTime();
                    }
                }


            }
        }
    }
}
