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
using System.Collections.Specialized;

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

            Updater updater = new Updater();
            bool isUpToDate = await updater.IsUpToDate();

            if (!isUpToDate)
            {
                MessageBoxResult result = MessageBox.Show("New update is available. \n\nDownload now?", "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if(result == MessageBoxResult.Yes)
                {
                    SettingsManager.SaveAllSettings(GbxGameController);
                    updater.DownloadUpdate();
                }
            }

            HistoryManager.UpdateListUI += HistoryManager_UpdateListUI;
        }

        private void HistoryManager_UpdateListUI(object sender, EventArgs e)
        {
            string tooltip = "";
            HistoryManager.HistoryListMinimal.ForEach(x => tooltip += x + "\n");
            tooltip = tooltip.Remove(tooltip.Length - 1);
            undoButton.ToolTip = tooltip;
            redoButton.ToolTip = tooltip;
        }

        #region GbxGameListInit
        void LoadGbxGameList()
        {
            gamesListMenu.DataContext = GbxGameController;
            openInComboBox.DataContext = GbxGameController;
            GbxGameController.LoadGames();
        }

        void ShowGbxGamesWindow()
        {
            GbxGamesWindow gbxGamesWindow = new GbxGamesWindow(GbxGameController);
            if(this.IsVisible && this is not null)
                gbxGamesWindow.Owner = this;
            gbxGamesWindow.ShowDialog();
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
            if (!selGame.IsVisibleInGameLaunchMenu) return;
            openInComboBox.SelectedItem = selGame;
            curFolder = selGame.MapsFolder;
            await UpdateMapList(selGame.MapsFolder);
            HistoryManager.AddToHistory(curFolder);
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
            UpdateMapPreview(null);
            MapInfoController.ClearMapList();

            //update enabled/disabled navigation buttons
            undoButton.IsEnabled = HistoryManager.CanUndo; 
            redoButton.IsEnabled = HistoryManager.CanRedo;


            Application.Current.Dispatcher.Invoke(() =>
            {
                //mapListBox.ItemsSource = null;
                currentFolderTextBox.Text = mapsFolder;
            }
            );
            string[] folders = await Task.Run(() => GetFolders(mapsFolder));
            var mapFiles = await Task.Run(() => GetMapPaths(mapsFolder));
            //var mapTasks = new Task[folders.Length + mapFiles.Length];
            int i = 0;

            foreach (var folderPath in folders)
            {
                await MapInfoController.AddFolder(folderPath);
                //mapTasks[i] = folderTask;
                i++;
            }
            foreach (string mapPath in mapFiles)
            {
                await MapInfoController.AddMap(mapPath);
                //mapTasks[i] = mapTask;
                i++;
            }

            await MapInfoController.SortMapList();

            mapListBox.ItemsSource = MapInfoController.MapList;

            Debug.WriteLine($"Items: {MapInfoController.MapList.Count}");
            mapListBox.Items.Refresh();

        }
        #endregion

        #region AdressBarButtonsEvents

        private async void sortMapsButton_Click(object sender, RoutedEventArgs e)
        {

            var sortMapsButtonTexts = new string[] { "Name ⬆️", "Name ⬇️", "Date ⬆️", "Date ⬇️", "Size ⬆️", "Size ⬇️", "Length ⬆️", "Length ⬇️" };
            if (MapInfoController.SortKind < (Sorting.Kind)7)
                MapInfoController.SortKind += 1;
            else MapInfoController.SortKind = 0;

            await UpdateMapList(curFolder);
            sortMapsButton.Content = "Sort by: " + sortMapsButtonTexts[(int)MapInfoController.SortKind];
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

        private async void undoButton_Click(object sender, RoutedEventArgs e)
        {
            curFolder = await Task.Run(HistoryManager.RequestPrev);
            await UpdateMapList(curFolder);
        }

        private async void redoButton_Click(object sender, RoutedEventArgs e)
        {
            curFolder = await Task.Run(HistoryManager.RequestNext);
            await UpdateMapList(curFolder);
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
            HistoryManager.AddToHistory(curFolder);

        }
        #endregion

        private async Task MapListBoxLaunchItemAsync(object selItem)
        {
            if (selItem is FolderInfo selFolder)
            {
                curFolder = selFolder.FolderFullPath;
                HistoryManager.AddToHistory(curFolder);
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

        private async void mapListBox_MouseDoubleClickAsync(object sender, MouseButtonEventArgs e)
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
        void MapPreview_SetPage(object data)
        {
            if (mapPreviewFrame.CanGoBack)
                mapPreviewFrame.RemoveBackEntry();
            mapPreviewFrame.Content = null;
            if (data == null) return;
            mapPreviewFrame.Content = (GbxInfoPage)data;
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
            if (selGame == null || !selGame.IsVisibleInGameLaunchMenu)
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
            if (mapListBox.SelectedItem is not MapInfo) return; //DOROB NA VIAC MAP
            if (e.MouseDevice.DirectlyOver is not TextBlock) return;
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
                                 where mappath.EndsWith("Map.Gbx") || mappath.EndsWith("Challenge.Gbx") || mappath.EndsWith("Replay.Gbx")
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
                HistoryManager.AddToHistory(curFolder);
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
                    var parentFolder = Directory.GetParent(curFolder);
                    if (parentFolder != null)
                        curFolder = parentFolder.FullName;
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.Message);
                }
                await UpdateMapList(curFolder);
                HistoryManager.AddToHistory(curFolder);
            }

            if (Keyboard.Modifiers == ModifierKeys.Alt && Keyboard.IsKeyDown(Key.Left))
                if(HistoryManager.CanUndo)
                    undoButton_Click(this, null);
            if (Keyboard.Modifiers == ModifierKeys.Alt && Keyboard.IsKeyDown(Key.Right))
                if (HistoryManager.CanRedo)
                    redoButton_Click(this, null);

            if (mapListBox.SelectedItem is not MapInfo) return; //must be selected map
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

        #region ContextMenus

        void ShowContextMenu()
        {
            if (mapListBox.SelectedItem == null) return;
            if (mapListBox.SelectedItem is not MapInfo) return;

            ContextMenu contextMenu = (ContextMenu)FindResource("MapContextMenu");
            contextMenu.PlacementTarget = mapListBox;
            contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
            contextMenu.IsOpen = true;
        }

        private void mapListBox_Item_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ((Grid)sender).ContextMenu.PreviewMouseUp += MapContextMenu_PreviewMouseUp;
        }

        private async void MapContextMenu_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mapListBox.SelectedItem is not MapInfo) return;
            if (!(e.Source is MenuItem)) return;
            
            var selMap = (MapInfo)mapListBox.SelectedItem;
            var selMenuItem = (MenuItem)e.Source;
            string path = selMap.MapFullName;
            e.Handled = true; //avoid running this code more than once

            switch (selMenuItem.Header)
            {
                case "Copy":
                    {
                        var fileDropList = new StringCollection();
                        fileDropList.Add(path);
                        Clipboard.SetFileDropList(fileDropList);
                        break;
                    }
                case "Paste":
                    {
                        var curFolder = FileOperations.GetFolderFromFilePath(path);
                        try
                        {
                            string[] clipboardText = null;
                            await Task.Run(() =>
                            Dispatcher.Invoke(() =>
                            clipboardText = (string[])Clipboard.GetDataObject().GetData(DataFormats.FileDrop)
                            )
                            );
                            FileOperations.CopyFilesToFolder(clipboardText, curFolder);
                            await UpdateMapList(curFolder);
                        }
                        catch { }
                        break;
                    }
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
                case "File Properties":
                    {
                        FileOperations.ShowFileProperties(path);
                        break;
                    }
                case "Map Properties (GBX Preview)":
                    {
                        var gbxInfoPage = new GbxInfoPage(path);
                        MapPreview_SetPage(gbxInfoPage);
                        break;
                    }              
            }
            await Task.Delay(100);
            ((ContextMenu)sender).IsOpen = false;
        }

        private void gameLibraryItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ((StackPanel)sender).ContextMenu.PreviewMouseUp += GameLibraryItemContextMenu_PreviewMouseUp;
        }

        private async void GameLibraryItemContextMenu_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is not MenuItem)
                return;

            var selMenuItem = (MenuItem)e.Source;
            e.Handled = true; //avoid running this multiple times
            if (selMenuItem == null)
                return;
            var selGame = gamesListMenu.SelectedItem;
            if (selGame is GbxGame game)
            {
                if (selMenuItem.Header.ToString() == "Hide from the game library")
                {
                    game.IsVisibleInGameList = false;
                    await Task.Run(() => SettingsManager.SaveAllSettings(GbxGameController));
                }
                else
                {
                    await Task.Run(() => game.Launch());
                }
                ((ContextMenu)sender).IsOpen = false;
                await Task.Delay(100);
            }

        }


        #endregion

        #region Search

        private async void searchMapsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = searchMapsTextBox.Text;
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            await MapInfoController.FindMaps(text);
            mapListBox.Items.Refresh();
        }

        private void searchMapsTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            searchMapsTextBox.Text = "";
            searchMapsTextBox.Opacity = .9;
        }

        private async void searchMapsTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            searchMapsTextBox.Opacity = .5;
            searchMapsTextBox.Text = "search for a map...";
            MapInfoController.ClearMapList();
            await UpdateMapList(curFolder);
        }
        #endregion

        private void sortMapsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
