using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spinnaker.Bridge.WPF
{
    [System.Runtime.InteropServices.ComVisible(true)]
    public class ScriptObject
    {
        private WPFBrowserBridge bridge;

        public ScriptObject(WPFBrowserBridge bridge)
        {
            this.bridge = bridge;
        }

        public void HandleScriptPropertyChanged(string id, string propertyName, string newValue)
        {
            bridge.HandleScriptPropertyChanged(id, propertyName, newValue);
        }

        public void InvokeViewModelMethod(string id, string methodName)
        {
            bridge.InvokeViewModelMethod(id, methodName);
        }

        public void InvokeViewModelIndexMethod(string id, string methodName, string arg)
        {
            bridge.InvokeViewModelMethod(id, methodName, arg);
        }

        public void HostLog(string msg)
        {
            bridge.HostLog("- JS - " + msg);
        }
    }
}
