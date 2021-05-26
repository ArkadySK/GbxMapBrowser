using System;
using System.Collections.Generic;
using System.Text;

namespace GbxMapBrowser
{
    class MapInfoController
    {
        public List<object> MapList { get; set; } = new List<object>();

        public void AddFolder(string fullnamepath)
        {
            FolderInfo folderInfo = new FolderInfo(fullnamepath);
            MapList.Add(folderInfo);
        }

        public void AddMap(string fullnamepath)
        {
            bool isOldTM = false;
            bool isNewTM = false;
            bool isReplay = false;
            if (fullnamepath.Contains("Challenge.Gbx", StringComparison.OrdinalIgnoreCase)) isOldTM = true;
            if (fullnamepath.Contains("Map.Gbx", StringComparison.OrdinalIgnoreCase)) isNewTM = true;
            if (fullnamepath.Contains("Replay.Gbx", StringComparison.OrdinalIgnoreCase)) isReplay = true;
            if (!isOldTM && !isNewTM && !isReplay) return;
            MapList.Add(new MapInfo(fullnamepath));
        }
        
        public void ClearMapList()
        {
            MapList.Clear();
        }
    }
}
