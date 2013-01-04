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
        private const string strConnectionString = @"isostore:/SmartfictionDB.sdf";
        public ObservableCollection<Story> Favorits { get; set; }

        // Constructor
        // http://code.msdn.microsoft.com/wpapps/Database-in-Windows-Phone-7-d69c13c9
        public MainPage()
        {
            InitializeComponent();

            this.Loaded += (s, e) =>
                               {
                                   if (App.Model.FeedItems.Count != 0)
                                       return;

                                   FeedHelper.FeedData.pb = new ProgressIndicator();
                                   SystemTray.SetProgressIndicator(this, FeedHelper.FeedData.pb);

                                   using (StoryDataContext context = new StoryDataContext(strConnectionString))
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

        private void RefreshFavorits()
        {
            using (StoryDataContext context = new StoryDataContext(strConnectionString))
            {
                Favorits = new ObservableCollection<Story>(context.Stories.ToList());
                FavoritsList.DataContext = Favorits; // todo fix this
            }
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Smartfiction.ViewModel.ItemModel item = (Smartfiction.ViewModel.ItemModel)((MenuItem)sender).DataContext;
            using (StoryDataContext context = new StoryDataContext(strConnectionString))
            {
                Story s = new Story();
                s.Title = item.Title;
                s.DateCreated = DateTime.Now;
                s.DatePublished = item.ItemPublishDate;
                s.Link = item.Link;
                s.Details = item.ItemDetails;

                context.Stories.InsertOnSubmit(s);

                context.SubmitChanges();
            }

            RefreshFavorits();
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

        private void RemoveFavorit_click(object sender, RoutedEventArgs e)
        {
            Story item = (Story)((MenuItem)sender).DataContext;
            using (StoryDataContext context = new StoryDataContext(strConnectionString))
            {
                context.Stories.Attach(item);
                context.Stories.DeleteOnSubmit(item);
                context.SubmitChanges();
            }

            RefreshFavorits();
        }

        private void about_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/YourLastAboutDialog;component/AboutPage.xaml", UriKind.Relative));
        }
    }
}
