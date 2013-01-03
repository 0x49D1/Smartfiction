using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using BugSense;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Newtonsoft.Json;
using Smartfiction.Model;

namespace Smartfiction
{
    public partial class DetailsView : PhoneApplicationPage
    {
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
                    WebClient wc = new WebClient();
                    wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webClient_OpenReadCompleted);
                    wc.DownloadStringAsync(new Uri(App.Model.FeedItems[_index].ItemLink + "?json=1"));
                }
                catch (Exception exception)
                {
                    BugSenseHandler.Instance.LogError(exception);
                }
                //WebBrowserTask wbt = new WebBrowserTask();
                //wbt.Uri = new Uri(App.Model.FeedItems[_index].ItemLink);
                //wbt.Show();

                //webBrowser1.Navigate(new Uri(App.Model.FeedItems[_index].ItemLink + "?json=1"));
            }
        }

        private void webClient_OpenReadCompleted(object sender, DownloadStringCompletedEventArgs e)
        {

            PostRoot value = JsonConvert.DeserializeObject<PostRoot>(e.Result);
            tbCaption.Text = value.post.title;
            webBrowser1.NavigateToString(value.post.content);
        }

        

        //private void webBrowser1_Navigating(object sender, NavigatingEventArgs e)
        //{
        //    ProgBar.Visibility = Visibility.Visible;
        //}

        //private void webBrowser1_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        //{
        //    ProgBar.Visibility = Visibility.Collapsed;
        //}

    }
}