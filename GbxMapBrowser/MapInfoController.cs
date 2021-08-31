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

        public void AddFolder(string fullnamepath)
        {
            FolderInfo folderInfo = new FolderInfo(fullnamepath);
            folderInfosList.Add(folderInfo);
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

        public async Task SortMapList(SortKind.Kind sortkind)
        {
            IOrderedEnumerable<MapInfo> orderedMapInfosList = null;
            switch (sortkind)
            {
                case SortKind.Kind.ByNameAscending:
                {
                    orderedMapInfosList = from map in mapInfosList
                                            orderby map.MapName ascending
                                            select map;
                    break;
                }
                case SortKind.Kind.ByNameDescending:
                {
                    orderedMapInfosList = from map in mapInfosList
                                            orderby map.MapName descending
                                            select map;
                    break;
                }
                case SortKind.Kind.ByDateModifiedAscending:
                {
                    orderedMapInfosList = from map in mapInfosList
                                            orderby map.DateModified ascending
                                            select map;
                    break;
                }
                case SortKind.Kind.ByDateModifiedDescending:
                {
                    orderedMapInfosList = from map in mapInfosList
                                            orderby map.DateModified descending
                                            select map;
                    break;
                }
                case SortKind.Kind.BySizeAscending:
                {
                    orderedMapInfosList = from map in mapInfosList
                                            orderby map.FileSize ascending
                                            select map;
                    break;
                }
                case SortKind.Kind.BySizeDescending:
                {
                    orderedMapInfosList = from map in mapInfosList
                                            orderby map.FileSize descending
                                            select map;
                    break;
                }
                case SortKind.Kind.ByLendthAscending:
                {
                    orderedMapInfosList = from map in mapInfosList
                                            orderby map.ObjectiveGold ascending
                                            select map;
                    break;
                }
                case SortKind.Kind.ByLendthDescending:
                {
                    orderedMapInfosList = from map in mapInfosList
                                            orderby map.ObjectiveGold descending
                                            select map;
                    break;
                }
            }
            
            mapList.Clear();
            mapList.AddRange(folderInfosList);
            await Task.Run(()=> mapList.AddRange(orderedMapInfosList));
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
