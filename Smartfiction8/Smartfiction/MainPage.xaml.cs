using System;
using System.Collections.ObjectModel;
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
            if (MainList.SelectedItems.Count != 0)
            {
                NavigationService.Navigate(new Uri("/DetailsView.xaml?item=" + MainList.SelectedIndex, UriKind.Relative));
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Smartfiction.ViewModel.ItemModel item = (Smartfiction.ViewModel.ItemModel)((MenuItem)sender).DataContext;
            using (StoryDataContext context = new StoryDataContext(strConnectionString))
            {
                Story s = new Story();
                s.Title = item.ItemTitle;
                s.DateCreated = DateTime.Now;
                s.DatePublished = item.ItemPublishDate;
                s.Link = item.ItemLink;
                s.Details = item.ItemDetails;

                context.Stories.InsertOnSubmit(s);

                context.SubmitChanges();
            }

            RefreshFavorits();
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
    }
}
