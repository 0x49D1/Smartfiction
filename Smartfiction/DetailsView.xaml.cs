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
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

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

                //WebBrowserTask wbt = new WebBrowserTask();
                //wbt.Uri = new Uri(App.Model.FeedItems[_index].ItemLink);
                //wbt.Show();
                webBrowser1.Navigate(new Uri(App.Model.FeedItems[_index].ItemLink));
            }
        }

        private void webBrowser1_Navigating(object sender, NavigatingEventArgs e)
        {
            ProgBar.Visibility = Visibility.Visible;
        }

        private void webBrowser1_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            ProgBar.Visibility = Visibility.Collapsed;
        }

    }
}