using Spinnaker.Core;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spinnaker.Log4net
{
    public class SpinnakerLog4netAdapter
    {
        private static ILog spinnakerLog = LogManager.GetLogger("Spinnaker");
        
        public static void Init()
        {
            SpinnakerConfiguration.CurrentConfig.LogHandler = (logLevel, msg, ex) =>
            {
                switch (logLevel)
                {
                    case SpinnakerLogLevel.Debug:
                        if (ex == null)
                            spinnakerLog.Debug(msg);
                        else
                            spinnakerLog.Debug(msg, ex);
                        break;
                    case SpinnakerLogLevel.Info:
                        if (ex == null)
                            spinnakerLog.Info(msg);
                        else
                            spinnakerLog.Info(msg,ex);
                        break;
                    case SpinnakerLogLevel.Warn:
                        if (ex == null)
                            spinnakerLog.Warn(msg);
                        else
                            spinnakerLog.Warn(msg, ex);
                        break;
                    case SpinnakerLogLevel.Error:
                        if (ex == null)
                            spinnakerLog.Error(msg);
                        else
                            spinnakerLog.Error(msg, ex);
                        break;
                }                
            };
        }
    }
}
