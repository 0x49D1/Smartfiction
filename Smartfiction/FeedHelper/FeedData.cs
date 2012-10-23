using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Syndication;
using System.Diagnostics;
using System.Xml;
using System.Net;
using System.IO;
using System.Text;

namespace Smartfiction.FeedHelper
{
    public class FeedData
    {
        public ObservableCollection<string> FeedList { get; set; }

        public static void GetItems()
        {
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
                                ItemTitle = sItem.Title.Text, 
                                ItemPublishDate =  sItem.PublishDate.DateTime,
                                ItemLink = sItem.Links[0].Uri.ToString()
                            });
                    }
                }
            }
        }
    }
}
