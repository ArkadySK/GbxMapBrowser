using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace GbxMapBrowser
{
    class MapOperations
    {

        public static void RenameMap(MapInfo mapInfo)
        {
            RenameWindow renameWindow = new RenameWindow(mapInfo.ExactMapName, false);
            renameWindow.ShowDialog();
            if (String.IsNullOrEmpty(renameWindow.newName)) return;
            mapInfo.RenameAndSave(renameWindow.newName);
        }

        public static void ChangeThumbnail(MapInfo mapInfo)
        {
            throw new NotImplementedException();
        }
    }
}
