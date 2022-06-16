using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GbxMapBrowser
{
    class MapInfoViewModel
    {
        public IReadOnlyList<object> MapList {
            get
            {
                return mapList.AsReadOnly();
            }
        }
        public Sorting.Kind SortKind { get; set; } = GbxMapBrowser.Sorting.Kind.ByNameAscending;


        private List<FolderInfo> folderInfosList = new List<FolderInfo>();
        private List<MapInfo> mapInfosList = new List<MapInfo>();
        private List<object> mapList = new List<object>();

        public async Task AddFolder(string fullnamepath)
        {
            if (fullnamepath == null) return;
            FolderInfo folderInfo = await Task.Run(() => new FolderInfo(fullnamepath));
            folderInfosList.Add(folderInfo);
        }
        
        public async Task AddMap(string fullnamepath)
        {
            if (fullnamepath.Contains(".Map.Gbx", StringComparison.OrdinalIgnoreCase) || fullnamepath.Contains(".Replay.Gbx", StringComparison.OrdinalIgnoreCase) || fullnamepath.Contains(".Challenge.Gbx", StringComparison.OrdinalIgnoreCase))
            {
                MapInfo mapInfo = await Task.Run(() => new MapInfo(fullnamepath, true));
                mapInfosList.Add(mapInfo);
            }
        }

        public void ClearMapList()
        {
            mapList.Clear();
            folderInfosList.Clear();
            mapInfosList.Clear();
            GC.Collect();
        }

        public async Task FindMaps(string mapName)
        {
            IOrderedEnumerable<MapInfo> foundMapInfos = null;
            foundMapInfos = from map in mapInfosList
                            where map.Name.Contains(mapName, StringComparison.OrdinalIgnoreCase) 
                            orderby map.Name ascending
                            select map;
            mapList.Clear();
            await Task.Run(() => mapList.AddRange(foundMapInfos));
        }

        public async Task SortMapList()
        {
            IOrderedEnumerable<MapInfo> orderedMapInfos = null;
            IOrderedEnumerable<FolderInfo> orderedFolderInfos = null;
            switch (SortKind)
            {
                case Sorting.Kind.ByNameAscending:
                    orderedMapInfos = await Task.Run(() => (from map in mapInfosList
                                                            orderby map.Name ascending
                                            select map));
                    orderedFolderInfos = await Task.Run(() => (from folder in folderInfosList
                                                                orderby folder.Name ascending
                                        select folder));
                    break;
                case Sorting.Kind.ByNameDescending:
                    orderedMapInfos = await Task.Run(() => (from map in mapInfosList
                                                            orderby map.Name descending
                                        select map));
                    orderedFolderInfos = await Task.Run(() => (from folder in folderInfosList
                                                                orderby folder.Name descending
                                            select folder));
                    break;
                case Sorting.Kind.ByDateModifiedAscending:
                    orderedMapInfos = await Task.Run(() => (from map in mapInfosList
                                                            orderby map.DateModified ascending
                                            select map));
                    orderedFolderInfos = await Task.Run(() => (from folder in folderInfosList
                                                                orderby folder.DateModified ascending
                                            select folder));
                    break;
                case Sorting.Kind.ByDateModifiedDescending:
                    orderedMapInfos = await Task.Run(() => (from map in mapInfosList
                                                            orderby map.DateModified descending
                                                            select map));
                    orderedFolderInfos = await Task.Run(() => (from folder in folderInfosList
                                                            orderby folder.DateModified descending
                                                            select folder));
                    break;
                case Sorting.Kind.BySizeAscending:
                    orderedMapInfos = await Task.Run(() => (from map in mapInfosList
                                                            orderby map.Size ascending
                                                            select map));
                    break;
                case Sorting.Kind.BySizeDescending:
                    orderedMapInfos = await Task.Run(() => (from map in mapInfosList
                                                            orderby map.Size descending
                                                            select map));
                    break;
                case Sorting.Kind.ByTitlepackAscending:
                    orderedMapInfos = await Task.Run(() => (from map in mapInfosList
                                                            orderby map.Titlepack ascending
                                                            select map));
                    break;
                case Sorting.Kind.ByTitlepackDescending:
                    orderedMapInfos = await Task.Run(() => (from map in mapInfosList
                                                            orderby map.Titlepack descending
                                                            select map));
                    break;
                case Sorting.Kind.ByLengthAscending:
                    orderedMapInfos = await Task.Run(() => (from map in mapInfosList
                                                            orderby map.ObjectiveGold ascending
                                                            select map));
                    break;
                case Sorting.Kind.ByLengthDescending:
                    orderedMapInfos = await Task.Run(() => (from map in mapInfosList
                                                            orderby map.ObjectiveGold descending
                                                            select map));
                    break;
            }

            if(orderedFolderInfos == null) //fix when null
            {
                orderedFolderInfos = await Task.Run(() => from folder in folderInfosList
                                                          orderby folder.Name ascending
                                                          select folder);
            }
            
            mapList.Clear();
            await Task.Run(() => mapList.AddRange(orderedFolderInfos));
            await Task.Delay(2);
            await Task.Run(() => mapList.AddRange(orderedMapInfos));
        }

        public MapInfo[] GetMapsByName(string[] mapNames)
        {
            List<MapInfo> maps = new List<MapInfo>();
            foreach (var mi in MapList)
            {
                if (!(mi is MapInfo)) continue;
                var mapName = (mi as MapInfo).Name;
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
