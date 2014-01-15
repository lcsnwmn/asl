using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AudioStreamListener
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var log = LogManager.GetLogger(typeof(AudioStreamListener));

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new AudioStreamListener(log) 
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
