﻿using System;
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

        private const string RemoveFromFavoritsString = "Удалить из избранного";
        private const string AddToFavoritsString = "Добвить в избранное";

        public DetailsView()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(DetailsView_Loaded);
            ContentWebBrowser.ScriptNotify += ContentWebBrowser_ScriptNotify;
        }

        void DetailsView_Loaded(object sender, RoutedEventArgs e)
        {
            string itemURL = "";
            string randURI = "";
            string title = "";
            pi.IsIndeterminate = true;
            pi.IsVisible = true;
            SystemTray.SetProgressIndicator(this, pi);

            if (NavigationContext.QueryString.TryGetValue("title", out title))
            {
                var story = StoryRepository.GetSingleStoryByTitle(title);
                if (story != null && !story.Details.Contains("[..."))
                {
                    value = new PostRoot();
                    value.post = new Post();
                    value.post.title = story.Title;
                    value.post.url = story.Link;
                    ContentWebBrowser.NavigateToString(InjectedString(story.Details));
                    pi.IsVisible = false;
                    var caption = story.Title.Split('.');
                    tbCaption.Text = caption[0];
                    if (caption.Length > 1)
                        tbCaptionAuthor.Text = caption[1].Trim();
                    return;
                }
            }

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
                    BugSenseHandler.Instance.LogException(exception);
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
                    BugSenseHandler.Instance.LogException(exception);
                    pi.IsVisible = false;
                }
            }
        }

        private string InjectedString(string content)
        {
            content = content.Replace("<p", "<p class='hyphenate'");
            return "<html lang='ru'><head><script src='./Html/Hyphenator.js' type='text/javascript'></script><meta name='viewport' content='width=400, initial-scale=1,maximim-scale=1.4'></head>" + JSInjectionScript + "<body><style>" + css + "</style>" + "<div class='wrapper'>" + content + "<div class='end'>&diams; &diams; &diams;</div></div></body></html>";
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
                                           //if (NavigationContext.QueryString["b"] != null)
                                           //{
                                           //    value.post.content = "<div style='background-color:black;color:white;margin:0;padding:0'>" + value.post.content + "</div>";
                                           //ContentWebBrowser.Background = new SolidColorBrush(Colors.Black);
                                           //}

                                           ContentWebBrowser.NavigateToString(InjectedString(value.post.content));
                                           pi.IsVisible = false;

                                           ApplicationBarMenuItem mi = ApplicationBar.MenuItems[1] as ApplicationBarMenuItem;
                                           if (mi != null)
                                               mi.Text = StoryRepository.CheckStoryTitle(value.post.title) ? RemoveFromFavoritsString : AddToFavoritsString;
                                       });

        }

        private void share_Click(object sender, EventArgs e)
        {
            if (!Utilities.CheckNetwork())
                return;
            ShareLinkTask slt = new ShareLinkTask();

            slt.LinkUri = new Uri(value.post.url);
            slt.Title = value.post.title;
            slt.Message = value.post.title + " #smartfiction #wp";
            slt.Show();
        }

        private void favorit_Click(object sender, EventArgs e)
        {
            // Add/Remove from favorits. depending on situation and change text on menu item accordinlgy
            ApplicationBarMenuItem mi = ApplicationBar.MenuItems[1] as ApplicationBarMenuItem;
            if (mi == null)
                return;
            if (StoryRepository.CheckStoryTitle(value.post.title))
            {
                if (StoryRepository.RemoveStory(value.post.title))
                    mi.Text = AddToFavoritsString;
            }
            else
            {
                if (StoryRepository.AddNewStory(value.post.title,
                                                DateTime.Parse(value.post.date),
                                                value.post.url,
                                                value.post.content, null) > 0)
                    mi.Text = RemoveFromFavoritsString;
            }
        }

        private void nightMode_Click(object sender, EventArgs e)
        {
            var item = (ApplicationBarMenuItem)sender;

            if (Math.Abs(ContentWebBrowser.Opacity - 1) < 0.1)
            {
                ContentWebBrowser.Opacity = 0.65;
                item.Text = "Дневной режим";
            }
            else
            {
                ContentWebBrowser.Opacity = 1;
                item.Text = "Ночной режим";
            }
        }

        private void invert_Click(object sender, EventArgs e)
        {
            //SolidColorBrush b = new SolidColorBrush(Colors.White);
            //if (ContentWebBrowser.Background != null)
            //    b = (SolidColorBrush)ContentWebBrowser.Background;
            //if (b.Color == Colors.White)
            //{
            //    b.Color = Colors.Black;
            //    ContentWebBrowser.Background = b;
            //    SolidColorBrush f = new SolidColorBrush(Colors.White);
            //    ContentWebBrowser.Foreground = f;
            //}
            //else
            //{
            //    b.Color = Colors.White;
            //    ContentWebBrowser.Background = b;
            //    SolidColorBrush f = new SolidColorBrush(Colors.Black);
            //    ContentWebBrowser.Foreground = f;
            //}

        }

        private void copy_Click(object sender, EventArgs e)
        {
            if (value.post != null)
                Clipboard.SetText(value.post.title + " " + value.post.url);
        }

        #region Dirty hack to show scrollbar in webrowser control with javascript injection..

        private string css =
            @".wrapper {font-family:Georgia;font-size:14pt;line-height:24pt;color:#222;margin:0px 3% 0px 3%; width:98%;}
p {text-indent:30px;-ms-hyphens: auto;hyphens: auto;}
div.end {text-align:center;color:#999;font-size:150%;margin-top:30px;width:100%;}";

        private string JSInjectionScript = @"<script>function initialize() { 
  window.external.notify('scrollHeight=' + document.body.scrollHeight.toString()); 
  window.external.notify('clientHeight=' + document.body.clientHeight.toString()); 
  window.onscroll = onScroll; 
}
 
function onScroll(e) { 
  var scrollPosition = document.body.scrollTop; 
  window.external.notify('scrollTop=' + scrollPosition.toString()); 
}
 
window.onload = initialize;
    Hyphenator.run();
</script>";

        private int _visibleHeight = 0;

        private int _scrollHeight = 0;

        private void ContentWebBrowser_ScriptNotify(object sender, NotifyEventArgs e)
        {
            // todo possibly you can add some code to savecurrent position here.
            // split 
            var parts = e.Value.Split('=');
            if (parts.Length != 2)
            {
                return;
            }

            // parse
            int number = 0;
            if (!int.TryParse(parts[1], out number))
            {
                return;
            }

            // decide what to do
            if (parts[0] == "scrollHeight")
            {
                _scrollHeight = number;
                if (_visibleHeight > 0)
                {
                    DisplayScrollBar.Maximum = _scrollHeight - _visibleHeight;
                }
            }
            else if (parts[0] == "clientHeight")
            {
                _visibleHeight = number;
                if (_scrollHeight > 0)
                {
                    DisplayScrollBar.Maximum = _scrollHeight - _visibleHeight;
                }
            }
            else if (parts[0] == "scrollTop")
            {
                DisplayScrollBar.Value = number;
            }
        }

        #endregion
    }
}