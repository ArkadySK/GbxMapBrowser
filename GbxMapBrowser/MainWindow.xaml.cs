using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;
using System.ArrayExtensions;

namespace GbxMapBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MapInfoController MapInfoController = new MapInfoController();
        GbxGameController GbxGameController = new GbxGameController();
        string curFolder;
        SearchOption searchOption;

         public MainWindow()
        {
            curFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            searchOption = SearchOption.TopDirectoryOnly;
            InitializeComponent();
            LoadGbxGameList();
            UpdateMapList(curFolder);
            UpdateMapPreviewVisibility(Properties.Settings.Default.ShowMapPreviewColumn);
            //Properties.Settings.Default.IsFirstRun = true;

            if (Properties.Settings.Default.IsFirstRun)
            {
                ShowGbxGamesWindow();
            }
        }

        void UpdateMapPreviewVisibility(bool isVis)
        {
            if(isVis)
                mapPreviewColumn.Width = new GridLength(1, GridUnitType.Star);
            else
                mapPreviewColumn.Width = new GridLength(0, GridUnitType.Star);
        }

        #region GbxGameListInit
        void LoadGbxGameList()
        {
            gamesListMenu.ItemsSource = GbxGameController.GbxGames;
            openInComboBox.ItemsSource = GbxGameController.GbxGames;
            GbxGameController.AddGbxGame("TM Forever", Properties.Settings.Default.TMForeverFolder, "TmForever.exe");
            GbxGameController.AddGbxGame("ManiaPlanet", Properties.Settings.Default.ManiaPlanetFolder, "ManiaPlanet.exe");
            GbxGameController.AddGbxGame("TM 2020", Properties.Settings.Default.TMNextFolder, "Trackmania.exe");
        }

        void ShowGbxGamesWindow()
        {
            GbxGamesWindow gbxGamesWindow = new GbxGamesWindow(GbxGameController);
            gbxGamesWindow.ShowDialog();
            gamesListMenu.ItemsSource = null;
            openInComboBox.ItemsSource = null;
            gamesListMenu.ItemsSource = GbxGameController.GbxGames;
            openInComboBox.ItemsSource = GbxGameController.GbxGames;
        }
        #endregion

        #region GbxGameListMethods
        private void manageGamesButton_Click(object sender, RoutedEventArgs e)
        {
            ShowGbxGamesWindow();
        }

        private void gamesListMenu_ItemClick(object sender, MahApps.Metro.Controls.ItemClickEventArgs args)
        {
            if (gamesListMenu.SelectedItem == null) return;
            var selGame = (GbxGame)gamesListMenu.SelectedItem;
            if (!selGame.IsEnabled) return;
            openInComboBox.SelectedItem = selGame;
            curFolder = selGame.MapsFolder;
            UpdateMapList(selGame.MapsFolder);
            GbxGameController.SelectedGbxGame = selGame;
        }
        #endregion

        #region GetDataAndUpdateUI
        private string[] GetFolders(string folder) 
        {
            List<string> folders = new List<string>();
            try
            {
                var foldersarray = Directory.GetDirectories(folder);
                folders = foldersarray.ToList();
            }
            catch(Exception e)   {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);               
            }
            return folders.ToArray();
        }

        private string[] GetMapInfos(string folder)
        {
            List<string> mapInfos = new List<string>();
            try
            {
                var mapInfosarray = Directory.GetFiles(folder, "*.gbx", searchOption);
                mapInfos = mapInfosarray.ToList();
            }
            catch
            {             
            }
            return mapInfos.ToArray();
        }

        void UpdateMapList(string mapsFolder)
        {
            mapListView.ItemsSource = null;
            string[] folders = GetFolders(mapsFolder);

            var mapFiles = GetMapInfos(mapsFolder);
            currentFolderTextBox.Text = mapsFolder;
            MapInfoController.ClearMapList();

            foreach (var f in folders)
            {
                MapInfoController.AddFolder(f);
            }      
            foreach (string mapfullpath  in mapFiles)
            {
                MapInfoController.AddMap(mapfullpath);
            }
            mapListView.ItemsSource = MapInfoController.MapList;
        }

        #endregion

        #region AdressBarButtonsEvents
        private void refreshMapsButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateMapList(curFolder);
        }

        private void OpenInExplorerButton_Click(object sender, RoutedEventArgs e)
        {
            Process explorerProcess = new Process();
            explorerProcess.StartInfo = new ProcessStartInfo("explorer", curFolder);
            explorerProcess.Start();
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            curFolder = currentFolderTextBox.Text;
            UpdateMapList(curFolder);
        }

        private void parentFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var parentFolder = (Directory.GetParent(curFolder));
                if (parentFolder != null)
                    curFolder = parentFolder.FullName;
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
            UpdateMapList(curFolder);
        }
        #endregion

        private void mapListView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(mapListView.SelectedItem is FolderInfo)
            {
                curFolder = ((FolderInfo)mapListView.SelectedItem).FolderFullPath;
                UpdateMapList(curFolder);
            }
        }

        void OpenMap(string mapname)
        {
            var selGame = (GbxGame)openInComboBox.SelectedItem;
            if(selGame == null)
            {
                MessageBox.Show("Choose a game to launch your map with!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }

            string path = selGame.InstalationFolder + "\\" + selGame.TargetExeName;

            ProcessStartInfo gameGbxStartInfo = new ProcessStartInfo(path, "/useexedir /singleinst /file=\"" + mapname + "\"");
            //ProcessStartInfo gameGbxStartInfo = new ProcessStartInfo(path, "/useexedir /bench=\"C:\\Users\\Adam\\Documents\\Trackmania2020\\Replays\\My Replays\ClassicMod Showcase.Replay.Gbx\"");
            Process gameGbx = new Process();
            gameGbx.StartInfo = gameGbxStartInfo;
            gameGbx.Start();
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            if (mapListView.SelectedItem is MapInfo) 
                OpenMap(((MapInfo)mapListView.SelectedItem).MapFullName);
        }

        

        private void currentFolderTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                curFolder = currentFolderTextBox.Text;
                UpdateMapList(curFolder);
            }
        }

        #region DragOutMaps

        void DragOutMaps(MapInfo[] mapInfos)
        {
            List<string> files = new List<string>();
            Array.ForEach(mapInfos, mfo => files.Add(mfo.MapFullName));
                
            var mapFile = new DataObject(DataFormats.FileDrop, files.ToArray());
            DragDrop.DoDragDrop((DependencyObject)mapListView, mapFile, DragDropEffects.Copy);
        }

        private void mapListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            if (mapListView.SelectedItem == null) return;
            if (!(mapListView.SelectedItem is MapInfo)) return; //DOROB NA VIAC MAP
            if (!(e.MouseDevice.DirectlyOver is TextBlock)) return;
            string lastSelMapName = (e.MouseDevice.DirectlyOver as TextBlock).Text;
            List<string> mapNames = new List<string>();
            foreach (var m in mapListView.SelectedItems)
            {
                mapNames.Add((m as MapInfo).MapName);
            }
            if (!mapNames.Contains(lastSelMapName)) return;
            var selMaps = MapInfoController.GetMapsByName(mapNames.ToArray());

            if (MapInfoController.AtleastOneExists(selMaps))
                DragOutMaps(selMaps);
        }

        #endregion

        #region DragInMaps
        private void mapListView_Drop(object sender, DragEventArgs e)
        {
            string[] paths = (string[])(e.Data).GetData(DataFormats.FileDrop, false);
            var mapsPathsQuery = from mappath in paths
                                 where mappath.EndsWith("Map.Gbx") || mappath.EndsWith("Replay.Gbx")
                                 select mappath;
            var MapPathsArray = mapsPathsQuery.ToArray();
            if (MapPathsArray.Length == 0) return;

            FileOperations.CopyFilesToFolder(MapPathsArray, curFolder);
            UpdateMapList(curFolder);
        }
        #endregion

       
        #region MapOperations
        void DeleteMap(MapInfo mapInfo)
        {
            var messageBoxResult = MessageBox.Show($"Are you sure to delete {mapInfo.MapName} \nPath: {mapInfo.MapFullName}?", "Delete file?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                FileOperations.DeleteFile(mapInfo.MapFullName);
            }
        }

        void RenameMap(MapInfo mapInfo)
        {
            RenameWindow renameWindow = new RenameWindow(mapInfo.ExactMapName, false);
            renameWindow.ShowDialog();
            if (String.IsNullOrEmpty(renameWindow.newName)) return;
            mapInfo.RenameAndSave(renameWindow.newName);
        }
        #endregion

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (mapListView.SelectedItem == null) return;
            MapInfo selMap = (MapInfo)mapListView.SelectedItem;
            if (e.Key == Key.Delete)
            {
                DeleteMap(selMap);
                UpdateMapList(curFolder);
            }
            /* if(e.SystemKey == Key.F10)   //ALT + F10        
                 throw new NotImplementedException(); //context menu*/
            if (e.SystemKey == Key.F2)
            {
                RenameMap(selMap);
                UpdateMapList(curFolder);
            }
            if (e.SystemKey == Key.LeftAlt) //ALT + ENTER
                FileOperations.ShowFileProperties(selMap.MapFullName);
        }

        #region ContextMenuEvents

        private void ContextMenuDelete_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DeleteMap((MapInfo)mapListView.SelectedItem);
            UpdateMapList(curFolder);
        }

        private void ContextMenuRenameFile_MouseUp(object sender, MouseButtonEventArgs e)
        {
            string oldMapName = ((MapInfo)mapListView.SelectedItem).MapFullName;
            FileOperations.RenameFile(oldMapName);
            UpdateMapList(curFolder);
        }
        
        private void ContextMenuRenameMap_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MapInfo selMap = (MapInfo)mapListView.SelectedItem;
            RenameMap(selMap);
            UpdateMapList(curFolder);
        }

        private void ContextMenuProperties_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var path = ((MapInfo)mapListView.SelectedItem).MapFullName;
            FileOperations.ShowFileProperties(path);
        }
        #endregion
    }

}
