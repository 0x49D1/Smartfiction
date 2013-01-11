using System;
using System.Windows;
using LiveTileScheduledTaskAgent;
using Microsoft.Phone.Scheduler;

namespace Smartfiction.ViewModel
{
    public class ScheduleAgentReseter
    {
        private static string periodicTaskName = "SmartfictionTileUpdater";
        private static PeriodicTask periodicTask;

        public static void StartPeriodicAgent()
        {
            // is old task running, remove it
            periodicTask = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;
            if (periodicTask != null)
            {
                try
                {
                    ScheduledActionService.Remove(periodicTaskName);
                }
                catch (Exception)
                {
                }
            }
            // create a new task
            periodicTask = new PeriodicTask(periodicTaskName);
            // load description from localized strings
            periodicTask.Description = "Updater will update your story of the day tile.";
            // set expiration days
            periodicTask.ExpirationTime = DateTime.Now.AddDays(10);
            try
            {
                // add thas to scheduled action service
                ScheduledActionService.Add(periodicTask);
                // debug, so run in every 30 secs
#if(DEBUG_AGENT)
		ScheduledActionService.LaunchForTest(periodicTaskName, TimeSpan.FromSeconds(10));
		System.Diagnostics.Debug.WriteLine("Periodic task is started: " + periodicTaskName);
#endif

            }
            catch (InvalidOperationException exception)
            {
                if (exception.Message.Contains("BNS Error: The action is disabled"))
                {
                    // load error text from localized strings
                    MessageBox.Show("Background agents for this application have been disabled by the user.");
                }
                if (exception.Message.Contains("BNS Error: The maximum number of ScheduledActions of this type have already been added."))
                {
                    // No user action required. The system prompts the user when the hard limit of periodic tasks has been reached.
                }
            }
            catch (SchedulerServiceException)
            {
                // No user action required.
            }

            AgentStarter.ZeroLastCheckTime();
  
            AgentStarter.CheckTileTextUpdate();
        }
    }
}
