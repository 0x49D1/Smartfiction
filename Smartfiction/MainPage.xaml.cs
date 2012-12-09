using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Smartfiction.Model;

namespace Smartfiction
{
    public partial class MainPage : PhoneApplicationPage
    {
        private const string strConnectionString = @"isostore:/SmartfictionDB.sdf"; 
        // Constructor
        // http://code.msdn.microsoft.com/wpapps/Database-in-Windows-Phone-7-d69c13c9
        public MainPage()
        {
            InitializeComponent();

            using (StoryDataContext context = new StoryDataContext(strConnectionString))
            {
                if (context.DatabaseExists() == false)
                {
                    context.CreateDatabase();
                    MessageBox.Show("Story Database Created Successfully!!!");
                }
                else
                {
                    MessageBox.Show("Story Database already exists!!!");
                }
            } 

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
            Smartfiction.ViewModel.ItemModel item = (Smartfiction.ViewModel.ItemModel)((MenuItem)sender).DataContext;
            slt.LinkUri = new Uri(item.ItemLink);
            slt.Title = item.ItemTitle;
            slt.Message = "";
            slt.Show();
        }
    }
}
