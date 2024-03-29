﻿using System;

namespace GbxMapBrowser
{
    class MapOperations
    {

        public static void RenameMap(MapInfo mapInfo)
        {
            RenameWindow renameWindow = new RenameWindow(mapInfo.OriginalName, false);
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
