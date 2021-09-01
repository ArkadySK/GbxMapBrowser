using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GbxMapBrowser
{
    class MapInfoController
    {
        public IReadOnlyList<object> MapList {
            get
            {
                return mapList.AsReadOnly();
            }
        }

        private List<FolderInfo> folderInfosList = new List<FolderInfo>();
        private List<MapInfo> mapInfosList = new List<MapInfo>();
        private List<object> mapList = new List<object>();

        public async void AddFolder(string fullnamepath)
        {
            FolderInfo folderInfo = new FolderInfo(fullnamepath);
            await Task.Run(()=>folderInfosList.Add(folderInfo));
        }

        public async Task AddMap(string fullnamepath)
        {
            if(fullnamepath.Contains(".Map.Gbx", StringComparison.OrdinalIgnoreCase) || fullnamepath.Contains(".Replay.Gbx", StringComparison.OrdinalIgnoreCase) || fullnamepath.Contains(".Challenge.Gbx", StringComparison.OrdinalIgnoreCase))
                await Task.Run(() => mapInfosList.Add(new MapInfo(fullnamepath, true)));
        }

        public void ClearMapList()
        {
            mapList.Clear();
            folderInfosList.Clear();
            mapInfosList.Clear();
        }

        public async Task FindMaps(string mapName)
        {
            IOrderedEnumerable<MapInfo> foundMapInfos = null;
            foundMapInfos = from map in mapInfosList
                            where map.MapName.Contains(mapName, StringComparison.OrdinalIgnoreCase) 
                            orderby map.MapName ascending
                            select map;
            mapList.Clear();
            await Task.Run(() => mapList.AddRange(foundMapInfos));
        }

        public async Task SortMapList(SortKind.Kind sortkind)
        {
            IOrderedEnumerable<MapInfo> orderedMapInfos = null; //has value everytime
            IOrderedEnumerable<FolderInfo> orderedFolderInfos = orderedFolderInfos = from folder in folderInfosList //value is changed only sometimes
                                                                                     orderby folder.FolderName ascending
                                                                                     select folder; 
            switch (sortkind)
            {
                case SortKind.Kind.ByNameAscending:
                {
                        orderedMapInfos = from map in mapInfosList
                                                orderby map.MapName ascending
                                                select map;
                        orderedFolderInfos = from folder in folderInfosList
                                            orderby folder.FolderName ascending
                                            select folder;
                        break;
                }
                case SortKind.Kind.ByNameDescending:
                {
                        orderedMapInfos = from map in mapInfosList
                                            orderby map.MapName descending
                                            select map;
                        orderedFolderInfos = from folder in folderInfosList
                                             orderby folder.FolderName descending
                                             select folder;
                        break;
                }
                case SortKind.Kind.ByDateModifiedAscending:
                {
                        orderedMapInfos = from map in mapInfosList
                                                orderby map.DateModified ascending
                                                select map;
                        orderedFolderInfos = from folder in folderInfosList
                                             orderby folder.DateModified ascending
                                             select folder;
                        break;
                }
                case SortKind.Kind.ByDateModifiedDescending:
                {
                        orderedMapInfos = from map in mapInfosList
                                                orderby map.DateModified descending
                                                select map;
                        orderedFolderInfos = from folder in folderInfosList
                                             orderby folder.DateModified descending
                                             select folder;
                        break;
                }
                case SortKind.Kind.BySizeAscending:
                {
                        orderedMapInfos = from map in mapInfosList
                                                orderby map.FileSize ascending
                                                select map;
                        break;
                }
                case SortKind.Kind.BySizeDescending:
                {
                        orderedMapInfos = from map in mapInfosList
                                                orderby map.FileSize descending
                                                select map;
                        break;
                }
                case SortKind.Kind.ByLendthAscending:
                {
                        orderedMapInfos = from map in mapInfosList
                                                orderby map.ObjectiveGold ascending
                                                select map;
                        break;
                }
                case SortKind.Kind.ByLendthDescending:
                {
                        orderedMapInfos = from map in mapInfosList
                                                orderby map.ObjectiveGold descending
                                                select map;
                        break;
                }
            }
            
            mapList.Clear();
            await Task.Run(() => mapList.AddRange(orderedFolderInfos));
            await Task.Run(() => mapList.AddRange(orderedMapInfos));
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
                if (mapList.Contains(mi)) return true;
            }
            return false;
        }
    }
}
