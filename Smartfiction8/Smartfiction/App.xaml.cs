using System;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Resources;
using BugSense;
using BugSense.Core.Model;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Phone.Tasks;
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
            BugSenseHandler.Instance.InitAndStartSession(new ExceptionManager(Current), RootFrame, "8279d9f3"); 

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

            //CopyToIsolatedStorage();
            //Utilities.hyphenator = OpenLocal("Hyphenator.js");

            //ADDED
            ViewModel.FeedItems = new ObservableCollection<ViewModel.ContentItem>();
            UnhandledException += Application_UnhandledException;
            InitializeComponent();
            InitializePhoneApplication();
        }


        private string OpenLocal(string path)
        {
            string rawData = "";
            using (IsolatedStorageFile appStorage =
                IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream isStream = new IsolatedStorageFileStream(path, FileMode.Open,
                                                                                       FileAccess.Read, appStorage))
                {
                    using (StreamReader sr = new StreamReader(isStream))
                    {
                        rawData = sr.ReadToEnd();
                    }
                }
            }

            return rawData;
        }

        private void CopyLocal(string path)
        {
            using (IsolatedStorageFile appStorage =
                IsolatedStorageFile.GetUserStoreForApplication())
            {
                var fileResourceStreamInfo = Application.GetResourceStream(new Uri("Html/"+path, UriKind.Relative));
                if (fileResourceStreamInfo != null)
                {
                    using (BinaryReader br = new BinaryReader(fileResourceStreamInfo.Stream))
                    {
                        byte[] data = br.ReadBytes((int)fileResourceStreamInfo.Stream.Length);

                        //string strBaseDir = path.Substring(0, path.LastIndexOf(System.IO.Path.DirectorySeparatorChar));

                        //if (!appStorage.DirectoryExists(strBaseDir))
                        //{
                        //    //Debug.WriteLine("Creating Directory :: " + strBaseDir);
                        //    appStorage.CreateDirectory(strBaseDir);
                        //}

                        // This will truncate/overwrite an existing file, or 
                        using (IsolatedStorageFileStream outFile = appStorage.OpenFile(path, FileMode.Create))
                        {
                            //Debug.WriteLine("Writing data for " + AppRoot + path + " and length = " + data.Length);
                            using (var writer = new BinaryWriter(outFile))
                            {
                                writer.Write(data);
                            }
                        }

                    }
                }
            }
        }

        private void CopyToIsolatedStorage()
        {
            using (IsolatedStorageFile storage =
                IsolatedStorageFile.GetUserStoreForApplication())
            {
                string[] files = new string[] 
                { 
                  "Hyphenator.js"
                };
                foreach (var _fileName in files)
                {
                    if (!storage.FileExists(_fileName))
                    {
                        string _filePath = "Html/" + _fileName;
                        StreamResourceInfo resource =
                            Application.GetResourceStream(
                            new Uri(_filePath, UriKind.Relative));

                        using (IsolatedStorageFileStream file =
                            storage.CreateFile(_fileName))
                        {
                            int chunkSize = 4096;
                            byte[] bytes = new byte[chunkSize];
                            int byteCount;

                            while ((byteCount =
                                resource.Stream.Read(
                                bytes, 0, chunkSize)) > 0)
                            {
                                file.Write(bytes, 0, byteCount);
                            }

                            file.Close();
                        }


                    }
                }
            }
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
            if (e != null)
                BugSenseHandler.Instance.LogException(e.ExceptionObject);
            SendLog("Unhandled exception " + e.ExceptionObject.Message + " stack: " + ((e.ExceptionObject.StackTrace != null) ? e.ExceptionObject.StackTrace.ToString() : ""));
            e.Handled = true;
        }

        public void SendLog(string message)
        {
            if (MessageBox.Show("Something went wrong. Do you want to send the log to developer?", "Log", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                string Subject = "Smartfiction WP LOG";

                try
                {
                    EmailComposeTask mail = new EmailComposeTask();
                    mail.Subject = Subject;
                    mail.To = "dpursanov@live.com;";
                    mail.Body = message;

                    if (mail.Body.Length > 32000) // max 32K 
                    {
                        mail.Body = mail.Body.Substring(mail.Body.Length - 32000);
                    }

                    mail.Show();
                }
                catch
                {
                    MessageBox.Show("Unable to create the email message");
                }
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
