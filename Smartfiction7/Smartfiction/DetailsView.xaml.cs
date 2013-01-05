using System;
using System.Net;
using System.Windows;
using BugSense;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Newtonsoft.Json;
using Smartfiction.Model;

namespace Smartfiction
{
    public partial class DetailsView : PhoneApplicationPage
    {
        ProgressIndicator pi = new ProgressIndicator();
        private PostRoot value;
        private static WebClient wc;

        public DetailsView()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(DetailsView_Loaded);
        }

        void DetailsView_Loaded(object sender, RoutedEventArgs e)
        {
            string itemURL = "";
            string randURI = "";
            pi.IsIndeterminate = true;
            pi.IsVisible = true;
            SystemTray.SetProgressIndicator(this, pi);

            wc = new WebClient();
            wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webClient_OpenReadCompleted);

            if (NavigationContext.QueryString.TryGetValue("item", out itemURL))
            {
                itemURL = HttpUtility.UrlDecode(itemURL);

                try
                {
                    wc.DownloadStringAsync(new Uri(itemURL + "?json=1"));
                }
                catch (Exception exception)
                {
                    BugSenseHandler.Instance.LogError(exception);
                    pi.IsVisible = false;
                }
            }
            if (NavigationContext.QueryString.TryGetValue("randURI", out randURI))
            {
                try
                {
                    string tURL = HttpUtility.UrlDecode(randURI);
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(HttpUtility.UrlDecode(randURI)));
                    request.Method = "HEAD";
                    request.AllowReadStreamBuffering = false;
                    // Start the asynchronous request.


                    request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);

                }
                catch (Exception exception)
                {
                    BugSenseHandler.Instance.LogError(exception);
                    pi.IsVisible = false;
                }
            }
        }

        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
            // End the operation
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);

            wc.DownloadStringAsync(new Uri(response.ResponseUri + "?json=1"));
        }

        private void webClient_OpenReadCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled)
                return;
            Dispatcher.BeginInvoke(() =>
                                       {
                                           value = JsonConvert.DeserializeObject<PostRoot>(e.Result);
                                           var caption = value.post.title.Split('.');
                                           tbCaption.Text = caption[0];
                                           if (caption.Length > 1)
                                               tbCaptionAuthor.Text = caption[1].Trim();
                                           webBrowser1.NavigateToString(value.post.content);
                                           pi.IsVisible = false;

                                           ApplicationBarMenuItem mi = ApplicationBar.MenuItems[1] as ApplicationBarMenuItem;
                                           if (mi != null)
                                               mi.Text = StoryRepository.CheckStoryTitle(value.post.title) ? "Удалить из избранного" : "Добвить в избранное";
                                       });

        }

        private void share_Click(object sender, EventArgs e)
        {
            ShareLinkTask slt = new ShareLinkTask();

            slt.LinkUri = new Uri(value.post.url);
            slt.Title = value.post.title;
            slt.Message = "";
            slt.Show();
        }

        private void favorit_Click(object sender, EventArgs e)
        {
            if (StoryRepository.CheckStoryTitle(value.post.title))
                StoryRepository.RemoveStory(value.post.title);
            else
                StoryRepository.AddNewStory(value.post.title,
                                                DateTime.Parse(value.post.date),
                                                value.post.url,
                                                value.post.excerpt);
        }

        private void nightMode_Click(object sender, EventArgs e)
        {
            var item = (ApplicationBarMenuItem)sender;

            if (Math.Abs(webBrowser1.Opacity - 1) < 0.1)
            {
                webBrowser1.Opacity = 0.65;
                item.Text = "Дневной режим";
            }
            else
            {
                webBrowser1.Opacity = 1;
                item.Text = "Ночной режим";
            }
        }

        private void copy_Click(object sender, EventArgs e)
        {
            if (value.post != null)
                Clipboard.SetText(value.post.title + " " + value.post.url);
        }
    }
}