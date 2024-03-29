﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using Common;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Smartfiction.Model;
using Smartfiction.ViewModel;

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
                                                "подождем?...",
                                                "наводимся на цель...",
                                                "ищем рассказы...",
                                                "ждем ответа..."
                                            };

        public static void GetItems()
        {
            ScheduleAgentReseter.StartPeriodicAgent();
            pb.IsIndeterminate = true;
            pb.IsVisible = true;
            pb.Text = l[new Random().Next(l.Count)]; // Get random loading captions

            App.ViewModel.FeedItems.Clear();

            GetFeed();
        }

        public static void GetFeed()
        {
            if (!Utilities.CheckNetwork())
                return;

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
                var value = JsonConvert.DeserializeObject<RootPostList>(e.Result);

                foreach (Post sItem in value.posts)
                {
                    if ((sItem != null) && (sItem.title != null))
                    {
                        App.ViewModel.FeedItems.Add(
                            new ViewModel.ContentItem()
                            {
                                ItemDetails = string.Format(" {0}", sItem.excerpt.Substring(sItem.excerpt.IndexOf("/*") + 2)),
                                Title = sItem.title,
                                ItemPublishDate = DateTime.Parse(sItem.date),
                                Link = sItem.url
                            });
                    }
                }
                pb.IsIndeterminate = false;
                pb.IsVisible = false;
            }
        }
    }
}
