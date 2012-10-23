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
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            FeedHelper.FeedData.GetItems();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            this.DataContext = App.Model;

            base.OnNavigatedTo(e);
        }

        private void manageFeeds_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ManageFeeds.xaml", UriKind.Relative));
        }

        private void reloadFeeds_Click(object sender, EventArgs e)
        {
            FeedHelper.FeedData.GetItems();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainList.SelectedItems.Count != 0)
            {
                NavigationService.Navigate(new Uri("/DetailsView.xaml?item=" + MainList.SelectedIndex, UriKind.Relative));
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("To favorits!");
        }

        private void ShareItem_Click(object sender, RoutedEventArgs e)
        {
            ShareLinkTask slt = new ShareLinkTask();
            slt.LinkUri = new Uri("http://test.test");
            slt.Title = "Title!";
            slt.Message = "Message";
            slt.Show();
        }
    }
}
