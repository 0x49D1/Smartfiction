using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Net;
using System.IO;
using Microsoft.Phone.Shell;

namespace Smartfiction.FeedHelper
{
    public class FeedData
    {
        public ObservableCollection<string> FeedList { get; set; }

        public static ProgressIndicator pb = new ProgressIndicator();

        private static List<string> l = new List<string>()
                                            {
                                                "загрузка...", 
                                                "подождите...", 
                                                "забираем последние...",
                                                "мартышки работают...",
                                                "нет, мы не чистим вашу память...",
                                                "уже скоро...",
                                                "совсем чуть чуть...",
                                                "подождем?..."
                                            };

        public static void GetItems()
        {
            pb.IsIndeterminate = true;
            pb.IsVisible = true;
            pb.Text = l[new Random().Next(l.Count)]; // Get random loading captions

            App.Model.FeedItems.Clear();

            foreach (string item in App.Data.FeedList)
            {
                WebClient client = new WebClient();

                client.Encoding = System.Text.Encoding.UTF8;

                client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
                client.DownloadStringAsync(new Uri(item));
            }
        }

        static void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Result))
            {
                XmlReader reader = XmlReader.Create(new StringReader(e.Result));
                SyndicationFeed feed = SyndicationFeed.Load(reader);

                foreach (SyndicationItem sItem in feed.Items)
                {
                    if ((sItem != null) && (sItem.Summary != null) && (sItem.Title != null))
                    {
                        App.Model.FeedItems.Add(
                            new ViewModel.ItemModel()
                            {
                                ItemDetails = sItem.Summary.Text,
                                Title = sItem.Title.Text,
                                ItemPublishDate = sItem.PublishDate.DateTime,
                                Link = sItem.Links[0].Uri.ToString()
                            });
                    }
                }
                pb.IsIndeterminate = false;
                pb.IsVisible = false;
            }
        }
    }
}
