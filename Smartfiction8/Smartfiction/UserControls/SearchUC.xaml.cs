using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Smartfiction.Model;

namespace Smartfiction.UserControls
{
    public partial class SearchUC : UserControl
    {
        public SearchUC()
        {
            InitializeComponent();
        }

        private void BtnSearch_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
                return;

            if (!Utilities.CheckNetwork())
                return;
            string url = HttpUtility.UrlEncode("http://smartfiction.ru/?json=get_search_results&search=" + txtSearch.Text);
            // todo finish the search
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            ((Popup) this.Parent).IsOpen = false;
        }
    }
}
