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
        private static List<string> HistoryList { get; } = new List<string>();
        public static List<string> HistoryListMinimal { 
            get 
            {
                List<string> list = new List<string>(); 
                for(int i = 0; i < 5; i++)
                {
                    if(i < HistoryList.Count)
                        list.Add(HistoryList[i]);
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

            if(HistoryList.Count > 0)
            {
                if(CurrentIndex < HistoryList.Count) //doterajsi pocet (curIndex - 1) je v ramci historylistu, potrebujem odstranit zbytkove itemy 
                    HistoryList.RemoveRange(CurrentIndex, HistoryList.Count - CurrentIndex);
            }

            HistoryList.Add(path);
            CanUndo = true;
            UpdateListUI?.Invoke(null, EventArgs.Empty);
        }


        public static string RequestPrev()
        {
            CurrentIndex -= 1; // go to -1 to the history
            if(CanUndo)
                try
                {
                    string pathToReturn = HistoryList[CurrentIndex];
                    CanRedo = true;
                    return pathToReturn;
                }
                catch {
                    throw new Exception("index out of range");
                }

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
                catch 
                {
                    throw new Exception("index out of range");
                }
            return null;
        }

    }
}
