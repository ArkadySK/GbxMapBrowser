namespace GbxMapBrowser
{
    internal class MapOperations
    {

        public static void RenameMap(MapInfo mapInfo)
        {
            RenameWindow renameWindow = new(mapInfo.OriginalName, false);
            renameWindow.ShowDialog();
            if (string.IsNullOrEmpty(renameWindow.NewName)) return;
            mapInfo.RenameAndSave(renameWindow.NewName);
        }
    }
}
