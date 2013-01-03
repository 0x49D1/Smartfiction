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
using System.IO.IsolatedStorage;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Smartfiction
{
    public partial class ManageFeeds : PhoneApplicationPage
    {
        public ManageFeeds()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            FeedHelper.FeedData.GetItems();
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            this.DataContext = App.Data;
            base.OnNavigatedTo(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(urlHolder.Text.Trim()))
            {
                if (!App.Data.FeedList.Contains(urlHolder.Text))
                {
                    if (Uri.IsWellFormedUriString(urlHolder.Text, UriKind.Absolute))
                    {
                        App.Data.FeedList.Add(urlHolder.Text);
                        urlHolder.Text = string.Empty;
                    }
                }
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (MainList.SelectedItem != null)
            {
                App.Data.FeedList.Remove(MainList.SelectedItem.ToString());
            }
        }
    }
}