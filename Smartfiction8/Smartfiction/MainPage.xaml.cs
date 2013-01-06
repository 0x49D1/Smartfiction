using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Smartfiction.Model;
using System.Linq;

namespace Smartfiction
{
    public partial class MainPage : PhoneApplicationPage
    {

        public ObservableCollection<Story> Favorits { get; set; }

        // Constructor
        // http://code.msdn.microsoft.com/wpapps/Database-in-Windows-Phone-7-d69c13c9
        public MainPage()
        {
            InitializeComponent();

            this.Loaded += (s, e) =>
                               {
                                   MainList.SelectedIndex = -1;
                                   FavoritsList.SelectedIndex = -1;

                                   if (App.Model.FeedItems.Count != 0)
                                   {
                                       RefreshFavorits();
                                       return;
                                   }
                                   FeedHelper.FeedData.pb = new ProgressIndicator();
                                   SystemTray.SetProgressIndicator(this, FeedHelper.FeedData.pb);

                                   using (StoryDataContext context = ConnectionFactory.GetStoryDataContext())
                                   {
                                       //context.DeleteDatabase();
                                       if (context.DatabaseExists() == false)
                                       {
                                           context.CreateDatabase();
                                       }
                                   }

                                   FavoritsList.DataContext = this.Favorits;

                                   FeedHelper.FeedData.GetItems();
                                   RefreshFavorits();
                               };
        }

        public void RefreshFavorits()
        {
            using (StoryDataContext context = ConnectionFactory.GetStoryDataContext())
            {
                Favorits = new ObservableCollection<Story>(context.Stories.OrderByDescending(s => s.DateCreated).ToList());
                FavoritsList.DataContext = Favorits; // todo fix this
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            this.DataContext = App.Model;

            base.OnNavigatedTo(e);
        }

        private void reloadFeeds_Click(object sender, EventArgs e)
        {
            FeedHelper.FeedData.GetItems();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedItems.Count != 0)
            {
                dynamic item = ((ListBox)sender).SelectedItem;
                string itemURL = HttpUtility.UrlEncode(item.Link);

                NavigationService.Navigate(new Uri("/DetailsView.xaml?item=" + itemURL, UriKind.Relative));
            }
        }

        private void randomFeeds_Click(object sender, EventArgs e)
        {
            string url = HttpUtility.UrlEncode("http://smartfiction.ru/random?random");
            NavigationService.Navigate(new Uri(string.Format("/DetailsView.xaml?randURI={0}", url), UriKind.Relative));
        }

        private void ShareItem_Click(object sender, RoutedEventArgs e)
        {
            ShareLinkTask slt = new ShareLinkTask();
            dynamic item = ((MenuItem)sender).DataContext;

            slt.LinkUri = new Uri(item.Link);
            slt.Title = item.Title;
            slt.Message = "";
            slt.Show();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Smartfiction.ViewModel.ItemModel item = (Smartfiction.ViewModel.ItemModel)((MenuItem)sender).DataContext;

            StoryRepository.AddNewStory(item.Title, item.ItemPublishDate, item.Link, item.ItemDetails);
            RefreshFavorits();
        }

        private void RemoveFavorit_click(object sender, RoutedEventArgs e)
        {
            Story item = (Story)((MenuItem)sender).DataContext;

            StoryRepository.RemoveStory(item);

            RefreshFavorits();
        }

        private void about_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/YourLastAboutDialog;component/AboutPage.xaml", UriKind.Relative));
        }

        private void copy_Click(object sender, RoutedEventArgs e)
        {
            dynamic item = ((MenuItem)sender).DataContext;

            Clipboard.SetText(item.Title + " " + item.Link);
        }
    }
}
