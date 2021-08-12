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
        string curFolder = "";
        MapInfoController MapInfoController = new MapInfoController();
        GbxGameController GbxGameController = new GbxGameController();
        SearchOption searchOption;

        public MainWindow()
        {
            curFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            searchOption = SearchOption.TopDirectoryOnly;
            InitializeComponent();
            LoadGbxGameList();
            UpdateMapPreviewVisibility(Properties.Settings.Default.ShowMapPreviewColumn);
            //Properties.Settings.Default.IsFirstRun = true;

            if (Properties.Settings.Default.IsFirstRun)
            {
                ShowGbxGamesWindow();
            }

        }

        private async void Window_LoadedAsync(object sender, RoutedEventArgs e)
        {
            await UpdateMapList(curFolder);
            
        }


        void UpdateMapPreviewVisibility(bool isVis)
        {
            if (isVis)
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

        private async void gamesListMenu_ItemClick(object sender, MahApps.Metro.Controls.ItemClickEventArgs args)
        {
            if (gamesListMenu.SelectedItem == null) return;
            var selGame = (GbxGame)gamesListMenu.SelectedItem;
            if (!selGame.IsEnabled) return;
            openInComboBox.SelectedItem = selGame;
            curFolder = selGame.MapsFolder;
            await UpdateMapList(selGame.MapsFolder);
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
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return folders.ToArray();
        }

        string[] GetMapPaths(string folder)
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

        async Task UpdateMapList(string mapsFolder)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Application.Current.Dispatcher.Invoke(() =>
            {
                mapListBox.ItemsSource = null;
                currentFolderTextBox.Text = mapsFolder;
            }
            );
            string[] folders = GetFolders(mapsFolder);
            var mapTasks = new List<Task>();
            var mapFiles = GetMapPaths(mapsFolder);
            MapInfoController.ClearMapList();

            foreach (var f in folders)
            {
                MapInfoController.AddFolder(f);
            }
            foreach (string mapfullpath in mapFiles)
            {
                Task mapTask = Task.Run(() => MapInfoController.AddMap(mapfullpath));
                mapTasks.Add(mapTask);
            }
            await Task.WhenAll(mapTasks.ToArray());
            await MapInfoController.SortMapList();
            mapListBox.ItemsSource = MapInfoController.MapList;

            sw.Stop();
            decimal s = sw.ElapsedMilliseconds / 1000m;
            Debug.WriteLine($"loaded in {s}s");
        }
        #endregion

        #region AdressBarButtonsEvents
        private async void refreshMapsButton_Click(object sender, RoutedEventArgs e)
        {
            await UpdateMapList(curFolder);
            Dispatcher.Invoke(() => mapListBox.ItemsSource = MapInfoController.MapList);
        }

        private void OpenInExplorerButton_Click(object sender, RoutedEventArgs e)
        {
            Process explorerProcess = new Process();
            explorerProcess.StartInfo = new ProcessStartInfo("explorer", curFolder);
            explorerProcess.Start();
        }

        private async void parentFolderButton_Click(object sender, RoutedEventArgs e)
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
            await UpdateMapList(curFolder);
            
        }
        #endregion

        private async void mapListBox_PreviewMouseDoubleClickAsync(object sender, MouseButtonEventArgs e)
        {
            if (mapListBox.SelectedItem is FolderInfo selFolder)
            {
                curFolder = selFolder.FolderFullPath;
                await UpdateMapList(curFolder);
                Dispatcher.Invoke(() => mapListBox.ItemsSource = MapInfoController.MapList);
            }
            else if (mapListBox.SelectedItem is MapInfo mapInfo)
            {
                var selGame = GetSelectedGame();
                if (selGame == null) return;
                mapInfo.OpenMap(selGame);
            }
        }

        private void mapListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(mapListBox.SelectedItem is MapInfo)) return;

            var selMap = (MapInfo)mapListBox.SelectedItem;
            var fullMap = new MapInfo(selMap.MapFullName, false);
            mapPreviewFrame.Content = new MapPreviewPage(fullMap);
        }

        GbxGame GetSelectedGame()
        {
            var selGame = (GbxGame)openInComboBox.SelectedItem;
            if (selGame == null)
            {
                MessageBox.Show("Choose a game to launch your map with!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                return null;
            }
            return selGame;

        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            if (!(mapListBox.SelectedItem is MapInfo)) return;
            if (GetSelectedGame() == null) return;
            (mapListBox.SelectedItem as MapInfo).OpenMap(GetSelectedGame());
        }


        private async void currentFolderTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                curFolder = currentFolderTextBox.Text;
                await UpdateMapList(curFolder);
                Dispatcher.Invoke(() => mapListBox.ItemsSource = MapInfoController.MapList);
            }
        }

        #region DragOutMaps

        void DragOutMaps(MapInfo[] mapInfos)
        {
            List<string> files = new List<string>();
            Array.ForEach(mapInfos, mfo => files.Add(mfo.MapFullName));

            var mapFile = new DataObject(DataFormats.FileDrop, files.ToArray());
            DragDrop.DoDragDrop((DependencyObject)mapListBox, mapFile, DragDropEffects.Copy);
        }

        private void mapListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            if (mapListBox.SelectedItem == null) return;
            if (!(mapListBox.SelectedItem is MapInfo)) return; //DOROB NA VIAC MAP
            if (!(e.MouseDevice.DirectlyOver is TextBlock)) return;
            string lastSelMapName = (e.MouseDevice.DirectlyOver as TextBlock).Text;
            List<string> mapNames = new List<string>();
            foreach (var m in mapListBox.SelectedItems)
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
        private async void mapListBox_Drop(object sender, DragEventArgs e)
        {
            string[] paths = (string[])(e.Data).GetData(DataFormats.FileDrop, false);
            var mapsPathsQuery = from mappath in paths
                                 where mappath.EndsWith("Map.Gbx") || mappath.EndsWith("Replay.Gbx")
                                 select mappath;
            var MapPathsArray = mapsPathsQuery.ToArray();
            if (MapPathsArray.Length == 0) return;

            FileOperations.CopyFilesToFolder(MapPathsArray, curFolder);
            await UpdateMapList(curFolder);
            
        }
        #endregion

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(mapListBox.SelectedItem is MapInfo)) return;
            MapInfo selMap = (MapInfo)mapListBox.SelectedItem;
            if (e.Key == Key.Delete)
            {
                MapOperations.DeleteMap(selMap);
                await UpdateMapList(curFolder);
                
            }
            /* if(e.SystemKey == Key.F10)   //ALT + F10        
                 throw new NotImplementedException(); //context menu*/
            if (e.SystemKey == Key.F2)
            {
                MapOperations.RenameMap(selMap);
                await UpdateMapList(curFolder);
                
            }
            if (e.SystemKey == Key.LeftAlt) //ALT + ENTER
                FileOperations.ShowFileProperties(selMap.MapFullName);
        }

        #region ContextMenuEvents

        private async void ContextMenuDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!(mapListBox.SelectedItem is MapInfo)) return;
            var selMap = (MapInfo)mapListBox.SelectedItem;
            await Task.Run(() => MapOperations.DeleteMap(selMap));
            await UpdateMapList(curFolder);
            
        }

        private async void ContextMenuRenameFile_Click(object sender, RoutedEventArgs e)
        {
            if (!(mapListBox.SelectedItem is MapInfo)) return;
            string oldMapName = ((MapInfo)mapListBox.SelectedItem).MapFullName;
            FileOperations.RenameFile(oldMapName);
            await UpdateMapList(curFolder);
            
        }

        private async void ContextMenuRenameMap_Click(object sender, RoutedEventArgs e)
        {
            if (!(mapListBox.SelectedItem is MapInfo)) return;
            MapInfo selMap = (MapInfo)mapListBox.SelectedItem;
            MapOperations.RenameMap(selMap);
            await UpdateMapList(curFolder);
            
        }

        private void ContextMenuProperties_Click(object sender, RoutedEventArgs e)
        {
            var path = ((MapInfo)mapListBox.SelectedItem).MapFullName;
            FileOperations.ShowFileProperties(path);
        }
        #endregion

        
    }
}
