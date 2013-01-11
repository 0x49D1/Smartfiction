using System.Windows;
using System.Windows.Navigation;
using BugSense;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Smartfiction.Model;

namespace Smartfiction
{
    public partial class App : Application
    {
        // Easy access to the root frame
        public PhoneApplicationFrame RootFrame { get; private set; }

        public static FeedHelper.FeedData Data;
        public static ViewModel.MainViewModel ViewModel;

        // Constructor
        public App()
        {
            BugSenseHandler.Instance.Init(this, "a9b557af");
            
            Data = new FeedHelper.FeedData();
            Data.FeedList = new ObservableCollection<string>();

            if (System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings.Count != 0)
            {
                foreach (string key in System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings.Keys)
                {
                    Data.FeedList.Add(System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings[key].ToString());
                    Debug.WriteLine(System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings[key].ToString());
                }
            }
            if (Data.FeedList.Count == 0)
                App.Data.FeedList.Add("http://smartfiction.ru?json=get_recent_posts&count=4");

            using (StoryDataContext context = ConnectionFactory.GetStoryDataContext())
            {
                //context.DeleteDatabase();
                if (context.DatabaseExists() == false)
                {
                    context.CreateDatabase();
                }
            }

            ViewModel = new ViewModel.MainViewModel();

            //ADDED
            ViewModel.FeedItems = new ObservableCollection<ViewModel.ContentItem>();
            BugSenseHandler.Instance.UnhandledException += Application_UnhandledException; 
            //UnhandledException += Application_UnhandledException;
            InitializeComponent();
            InitializePhoneApplication();
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            SaveList();
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            SaveList();
        }

        // Code to execute if a navigation fails
        void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion

        void SaveList()
        {
            IsolatedStorageSettings.ApplicationSettings.Clear();

            if (App.Data.FeedList.Count != 0)
            {
                foreach (string item in App.Data.FeedList)
                {
                    IsolatedStorageSettings.ApplicationSettings.Add(item, item);
                }
            }

            IsolatedStorageSettings.ApplicationSettings.Save();
        }
    }
}
