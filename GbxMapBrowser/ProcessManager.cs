using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GbxMapBrowser
{
    internal static class ProcessManager
    {
        public static bool IsRunning(string processName)
        {
            var processes = Process.GetProcesses();

            IEnumerable<Process> foundProcesses = from p in processes
                                                  where (p.ProcessName + ".exe").Equals(processName, StringComparison.CurrentCultureIgnoreCase)
                                                  select p;

            return foundProcesses.Any();
        }

        public static void OpenFile(string fileName)
        {
            string argument = "/open, \"" + fileName + "\"";
            Process.Start("explorer.exe", argument);
        }
    }
}
