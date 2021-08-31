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
        SortKind.Kind sortKind = SortKind.Kind.ByNameAscending;

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

        #region GbxGameListInit
        void LoadGbxGameList()
        {
            gamesListMenu.ItemsSource = GbxGameController.GbxGames;
            openInComboBox.ItemsSource = GbxGameController.GbxGames;
            GbxGameController.AddGbxGame("TM Forever", Properties.Settings.Default.TMForeverFolder, "TmForever.exe");
            GbxGameController.AddGbxGame("ManiaPlanet", Properties.Settings.Default.ManiaPlanetFolder, "ManiaPlanet.exe");
            GbxGameController.AddGbxGame("TM Turbo", Properties.Settings.Default.TMTurboFolder, "TrackmaniaTurbo.exe");
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
        string[] GetFolders(string folder)
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
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return mapInfos.ToArray();
        }

        async Task UpdateMapList(string mapsFolder)
        {
            MapInfoController.ClearMapList();
            UpdateMapPreview(null);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Application.Current.Dispatcher.Invoke(() =>
            {
                mapListBox.ItemsSource = null;
                currentFolderTextBox.Text = mapsFolder;
            }
            );
            string[] folders = await Task.Run(() => GetFolders(mapsFolder));
            var mapTasks = new List<Task>();
            var mapFiles = await Task.Run(() => GetMapPaths(mapsFolder));
            

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
            await MapInfoController.SortMapList(sortKind);
            mapListBox.ItemsSource = MapInfoController.MapList;

            sw.Stop();
            decimal s = sw.ElapsedMilliseconds / 1000m;
            Debug.WriteLine($"loaded in {s}s");
        }
        #endregion

        #region AdressBarButtonsEvents

        private async void sortMapsButton_Click(object sender, RoutedEventArgs e)
        {
            var sortMapsButtonTexts = new string[] { "AB ⬆️", "AB ⬇️", "📅 ⬆️", "📅 ⬇️", "MB ⬆️", "MB ⬇️", "⏱️ ⬆️", "⏱️ ⬇️"};
            if (sortKind < (SortKind.Kind)7)
                sortKind += 1;
            else sortKind = 0;

            await UpdateMapList(curFolder);
            sortMapsButton.Content = sortMapsButtonTexts[(int)sortKind];
        }

        private async void refreshMapsButton_Click(object sender, RoutedEventArgs e)
        {
            await UpdateMapList(curFolder);
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

        private async Task MapListBoxLaunchItemAsync(object selItem)
        {
            if (selItem is FolderInfo selFolder)
            {
                curFolder = selFolder.FolderFullPath;
                await UpdateMapList(curFolder);
                UpdateMapPreview(null);
            }
            else if (selItem is MapInfo mapInfo)
            {
                var selGame = GetSelectedGame();
                if (selGame == null) return;
                mapInfo.OpenMap(selGame);
            }
            else if (selItem is null)
                MessageBox.Show("Select a map to launch", "Impossible to load map", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private async void mapListBox_PreviewMouseDoubleClickAsync(object sender, MouseButtonEventArgs e)
        {
            await MapListBoxLaunchItemAsync(mapListBox.SelectedItem);
        }

        #region MapPreviewPane

        void UpdateMapPreviewVisibility(bool isVis)
        {
            if (isVis)
                mapPreviewColumn.Width = new GridLength(1, GridUnitType.Star);
            else
                mapPreviewColumn.Width = new GridLength(0, GridUnitType.Star);
        }

        void UpdateMapPreview(object data)
        {
            if (mapPreviewFrame.CanGoBack)
                mapPreviewFrame.RemoveBackEntry();
            mapPreviewFrame.Content = null;
            if (data == null) return;
            mapPreviewFrame.Content = new MapPreviewPage(data);
        }

        private void mapListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mapListBox.SelectedItem == null) return;
            var selMap = (object)mapListBox.SelectedItem;
            UpdateMapPreview(selMap);
        }

        GbxGame GetSelectedGame()
        {
            var selGame = (GbxGame)openInComboBox.SelectedItem;
            if (selGame == null || !selGame.IsEnabled)
            {
                MessageBox.Show("Choose a game to launch your map with!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                return null;
            }
            return selGame;
        }

        private async void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            await MapListBoxLaunchItemAsync(mapListBox.SelectedItem);
        }
        #endregion

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

        #region KeyPresses

        private async void currentFolderTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                curFolder = currentFolderTextBox.Text;
                await UpdateMapList(curFolder);
            }
        }

        private async void mapListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await MapListBoxLaunchItemAsync(mapListBox.SelectedItem);
            }

            if(e.Key == Key.Back)
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

            if (!(mapListBox.SelectedItem is MapInfo)) return; //must be selected map
            MapInfo selMap = (MapInfo)mapListBox.SelectedItem;

            if (e.Key == Key.Delete)
            {
                MapOperations.DeleteMap(selMap);
                await UpdateMapList(curFolder);

            }
            if (Keyboard.Modifiers == ModifierKeys.Shift && Keyboard.IsKeyDown(Key.F10))   //SHIFT + F10        
                ShowContextMenu(); //context menu
            if (e.SystemKey == Key.F2)
            {
                MapOperations.RenameMap(selMap);
                await UpdateMapList(curFolder);

            }
            if (Keyboard.IsKeyDown(Key.Enter) && Keyboard.Modifiers == ModifierKeys.Alt) //ALT + ENTER
                FileOperations.ShowFileProperties(selMap.MapFullName);
        }

        #endregion

        #region ContextMenu

        void ShowContextMenu()
        {
            if (mapListBox.SelectedItem == null) return;
            if (!(mapListBox.SelectedItem is MapInfo)) return;

            ContextMenu contextMenu = (ContextMenu)FindResource("MapContextMenu");
            contextMenu.PlacementTarget = mapListBox;
            contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
            contextMenu.IsOpen = true;
        }
 
        private void mapListBox_Item_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ContextMenu contextMenu = (ContextMenu)((Grid)sender).ContextMenu;
            contextMenu.PreviewMouseDown += ContextMenu_PreviewMouseDown;
        }

        private async void ContextMenu_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(mapListBox.SelectedItem is MapInfo)) return;
            if (!(e.Source is MenuItem)) return;
            
            var selMap = (MapInfo)mapListBox.SelectedItem;
            var selMenuItem = (MenuItem)e.Source;
            switch (selMenuItem.Header)
            {
                case "Delete":
                    {
                        MapOperations.DeleteMap(selMap);
                        await UpdateMapList(curFolder);
                        break;
                    }
                case "Rename File":
                    {
                        var oldMapName = selMap.MapFullName;
                        FileOperations.RenameFile(oldMapName);
                        await UpdateMapList(curFolder);
                        break;
                    }
                case "Rename Map":
                    {
                        MapOperations.RenameMap(selMap);
                        await UpdateMapList(curFolder);
                        break;
                    }
                case "Properties":
                    {
                        var path = ((MapInfo)mapListBox.SelectedItem).MapFullName;
                        FileOperations.ShowFileProperties(path);
                        break;
                    }
            }
        }
        #endregion

    }
}
