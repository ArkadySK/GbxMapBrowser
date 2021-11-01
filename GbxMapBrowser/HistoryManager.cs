using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GbxMapBrowser
{
    public static class HistoryManager
    {
        private static List<string> HistoryList { get; set; } = new List<string>();
        public static bool CanUndo { get; private set; } = false; 
        public static bool CanRedo { get; private set; } = false;
        static int CurrentIndex = -1;

        public static void AddToHistory(string path) 
        { 
            if (path == null) return; // do not add empty path, do nothing
            CanUndo = false;
            CanRedo = false;

            if (HistoryList.Count > 0)
                if (HistoryList.Last() == path) return; // skip if the path is already there (in the most recent pos)

            CurrentIndex += 1;
            if (CurrentIndex < HistoryList.Count)
                Console.WriteLine(path);
            else
                HistoryList.Add(path);

            if (CurrentIndex > 0)
                CanUndo= true;
            if (CurrentIndex < HistoryList.Count - 1)
                CanRedo = true;
        }

        public static string RequestPrev()
        {
            CurrentIndex -= 1; // go to -1 to the history
            if(CanUndo)
                try
                {
                    string pathToReturn = HistoryList[CurrentIndex];
                    CurrentIndex -= 1;
                    return pathToReturn;
                }
                catch { }

            return null;
        }

        public static string RequestNext()
        {
            CurrentIndex += 1;
            if(CanRedo) 
            try
            {
                string pathToReturn = HistoryList[CurrentIndex];
                return pathToReturn;
            }
            catch { }
            return null;
        }

    }
}
