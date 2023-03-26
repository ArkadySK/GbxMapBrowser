using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GbxMapBrowser
{
    public static class HistoryManager
    {
        static public event EventHandler UpdateListUI;
        private static List<string> historyList = new List<string>();
        public static List<string> HistoryListMinimal { 
            get 
            {
                List<string> list = new List<string>(); 
                for(int i = 0; i < 5; i++)
                {
                    if(i < historyList.Count)
                        list.Add(historyList[i]);
                }
                return list;
            } 
        }
        public static bool CanUndo { get; private set; } = false; 
        public static bool CanRedo { get; private set; } = false;
        static int CurrentIndex = -1;

        public static void AddToHistory(string path)
        {
            if (string.IsNullOrEmpty(path)) return; // do not add empty path, do nothing

            CurrentIndex++;      

            if(historyList.Count > 0)
            {
                if(CurrentIndex < historyList.Count) //remove all other items from previous history branch
                    historyList.RemoveRange(CurrentIndex, historyList.Count - CurrentIndex);
            }

            historyList.Add(path);
            CheckUndoRedo();

            UpdateListUI?.Invoke(null, EventArgs.Empty);
        }

        private static void CheckUndoRedo()
        {
            CanUndo = false;
            CanRedo = false;
            if (CurrentIndex > 0)
                CanUndo = true;
            if(CurrentIndex < historyList.Count - 1)
                CanRedo = true; 
        }

        public static string RequestPrev()
        {
            CurrentIndex -= 1; // go to -1 to the history
            if (CanUndo)
                try
                {
                    CheckUndoRedo();
                    string pathToReturn = historyList[CurrentIndex];
                    return pathToReturn;
                }
                catch {}

            return null;
        }

        public static string RequestNext()
        {
            CurrentIndex += 1;

            if(CanRedo) 
                try
                {
                    CheckUndoRedo();
                    string pathToReturn = historyList[CurrentIndex];
                    return pathToReturn;
                }
                catch {}
            return null;
        }

    }
}
