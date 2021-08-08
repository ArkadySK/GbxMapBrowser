using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task AddMap(string fullnamepath)
        {
            bool isOldTM = false;
            bool isNewTM = false;
            bool isReplay = false;
            if (fullnamepath.Contains("Challenge.Gbx", StringComparison.OrdinalIgnoreCase)) isOldTM = true;
            if (fullnamepath.Contains("Map.Gbx", StringComparison.OrdinalIgnoreCase)) isNewTM = true;
            if (fullnamepath.Contains("Replay.Gbx", StringComparison.OrdinalIgnoreCase)) isReplay = true;
            if (!isOldTM && !isNewTM && !isReplay) return;
            await Task.Run(() => MapList.Add(new MapInfo(fullnamepath)));
        }
        
        public void ClearMapList()
        {
            MapList.Clear();
        }


        public MapInfo[] GetMapsByName(string[] mapNames)
        {
            List<MapInfo> maps = new List<MapInfo>();
            foreach (var mi in MapList)
            {
                if (!(mi is MapInfo)) continue;
                var mapName = (mi as MapInfo).MapName;
                if (mapNames.ToList().Contains(mapName)) maps.Add((MapInfo)mi);
            }
            return maps.ToArray();
        }

        public bool AtleastOneExists(MapInfo[] mapInfos) 
        {
            foreach (var mi in mapInfos)
            {
                if (MapList.Contains(mi)) return true;
            }
            return false;
        }
    }
}
