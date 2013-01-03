using System;
using System.Net;
using System.Windows;
using BugSense;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Smartfiction.Model;

namespace Smartfiction
{
    public partial class DetailsView : PhoneApplicationPage
    {
        ProgressIndicator pi = new ProgressIndicator();

        public DetailsView()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(DetailsView_Loaded);
        }

        void DetailsView_Loaded(object sender, RoutedEventArgs e)
        {
            string index = "";

            if (NavigationContext.QueryString.TryGetValue("item", out index))
            {
                int _index = int.Parse(index);

                try
                {
                    pi.IsIndeterminate = true;
                    pi.IsVisible = true;
                    SystemTray.SetProgressIndicator(this, pi);

                    WebClient wc = new WebClient();
                    wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webClient_OpenReadCompleted);
                    wc.DownloadStringAsync(new Uri(App.Model.FeedItems[_index].ItemLink + "?json=1"));
                }
                catch (Exception exception)
                {
                    BugSenseHandler.Instance.LogError(exception);
                    pi.IsVisible = false;
                }
            }
        }

        private void webClient_OpenReadCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            PostRoot value = JsonConvert.DeserializeObject<PostRoot>(e.Result);
            tbCaption.Text = value.post.title;
            webBrowser1.NavigateToString(value.post.content);
            pi.IsVisible = false;
        }
    }
}