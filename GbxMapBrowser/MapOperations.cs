using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace GbxMapBrowser
{
    class MapOperations
    {
        public static void DeleteMap(MapInfo mapInfo)
        {
            var messageBoxResult = MessageBox.Show($"Are you sure to delete {mapInfo.Name} \nPath: {mapInfo.FullPath}?", "Delete file?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                FileOperations.DeleteFile(mapInfo.FullPath);
            }
        }

        public static void RenameMap(MapInfo mapInfo)
        {
            RenameWindow renameWindow = new RenameWindow(mapInfo.ExactMapName, false);
            renameWindow.ShowDialog();
            if (String.IsNullOrEmpty(renameWindow.newName)) return;
            mapInfo.RenameAndSave(renameWindow.newName);
        }
    }
}
