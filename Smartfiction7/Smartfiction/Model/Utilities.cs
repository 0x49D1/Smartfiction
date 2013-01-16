using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using Microsoft.Phone.Net.NetworkInformation;

namespace Smartfiction.Model
{
    public static class Utilities
    {
     public static string hyphenator = "";


        /// <summary>
        /// Method to check internet connection to the host
        /// </summary>
        /// <returns></returns>
        public static bool CheckNetwork()
        {
            var manualResetEvent = new ManualResetEvent(false);
            bool internetConnectionAvailable = true;
            DeviceNetworkInformation.ResolveHostNameAsync(new DnsEndPoint("smartfiction.ru", 80),
                                                          networkInfo =>
                                                          {
                                                              if (networkInfo.NetworkInterface == null)
                                                              {
                                                                  internetConnectionAvailable = false;
                                                              }
                                                              manualResetEvent.Set();
                                                          }, null);
            manualResetEvent.WaitOne(TimeSpan.FromMilliseconds(50));
            if (!internetConnectionAvailable)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                                                                             {
                                                                                 MessageBox.Show(
                                                                                     "Так уж вышло, что для некоторых действий требуется интернет! В данный момент программа не может соединиться с smartfiction.ru",
                                                                                     "Warning", MessageBoxButton.OK);

                                                                                 //one can use here new Game().Exit()
                                                                                 //throw new Exception(
                                                                                 //    "ExitApplicationException");
                                                                             });

                return false;
            }
            return true;
        }
    }
}
