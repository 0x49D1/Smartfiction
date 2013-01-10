using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;

namespace LiveTileScheduledTaskAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            
                // get application tile
                ShellTile tile = ShellTile.ActiveTiles.First();
                if (null != tile)
                {
                    // creata a new data for tile
                    StandardTileData data = new StandardTileData();
                    // tile foreground data
                    data.Title = "";
                    data.BackgroundImage = new Uri("/Images/logo.jpg", UriKind.Relative);

                    // to make tile flip add data to background also
                    data.BackTitle = "";
                    // For white theme show white square
                    data.BackBackgroundImage =
                        //((Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible)
                        new Uri("/Images/black.png", UriKind.Relative);
                            //: new Uri("/Images/white.png", UriKind.Relative);
                    data.BackContent = task.Description.Substring(0, task.Description.IndexOf("is"));
                    // take just TITLE from description
                    // update tile
                    tile.Update(data);
                }
#if DEBUG_AGENT
	ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(30));
	System.Diagnostics.Debug.WriteLine("Periodic task is started again: " + task.Name);
#endif

            NotifyComplete();
        }
    }
}