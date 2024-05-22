using System;

namespace GbxMapBrowser
{
    internal class MapOperations
    {

        public static void RenameMap(MapInfo mapInfo)
        {
            RenameWindow renameWindow = new(mapInfo.OriginalName, false);
            renameWindow.ShowDialog();
            if (String.IsNullOrEmpty(renameWindow.NewName)) return;
            mapInfo.RenameAndSave(renameWindow.NewName);
        }

        public static void ChangeThumbnail(MapInfo mapInfo)
        {
            throw new NotImplementedException();
        }
    }
}
