using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace GbxMapBrowser
{
    internal static class ProcessManager
    {
        public static bool IsRunning(string processName)
        {
            var processes = Process.GetProcesses();

            IEnumerable<Process> foundProcesses = from p in processes 
                        where (p.ProcessName + ".exe").ToLower() == processName.ToLower()
                        select p;

            if(foundProcesses.Count()== 0)
                return false;

            return true;
        }

        public static void OpenFile(string fileName)
        {
            string argument = "/open, \"" + fileName + "\"";
            Process.Start("explorer.exe", argument);
        }

        public static void StartProcess(string processName, string arguments)
        {
            Process.Start(processName, arguments);
        }
        
        public static void StartHiddenProcess(string processName, string arguments)
        {
            var pInfo = new ProcessStartInfo(processName, arguments);
            //pInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(pInfo);
        }
    }
}
