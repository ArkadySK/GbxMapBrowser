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
        MapInfoViewModel MapInfoController = new MapInfoViewModel();
        GbxGameViewModel GbxGameController = new GbxGameViewModel();
        SearchOption searchOption;
        List<FolderAndFileInfo> selectedItems = new List<FolderAndFileInfo> ();

        #region Initialization
        public MainWindow()
        {
            curFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            searchOption = SearchOption.TopDirectoryOnly;
            InitializeComponent();
            LoadGbxGameList();
            UpdateMapPreviewVisibility(Properties.Settings.Default.ShowMapPreviewColumn);
            LoadSorting();
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
            bool isUpToDate = true;

            try
            {
                isUpToDate = await updater.IsUpToDate();
            }
            catch { }

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
        #endregion

        #region HistoryManager
        private void HistoryManager_UpdateListUI(object sender, EventArgs e)
        {
            string tooltip = "";
            HistoryManager.HistoryListMinimal.ForEach(x => tooltip += x + "\n");
            tooltip = tooltip.Remove(tooltip.Length - 1);
            undoButton.ToolTip = tooltip;
            redoButton.ToolTip = tooltip;
        }
        #endregion

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
            if (!selGame.IsVisibleInGameList) return;

            // Assign selection of the game
            GbxGameController.SelectedGbxGame = selGame;
            openInComboBox.SelectedItem = selGame;

            // Assign sorting
            MapInfoController.SortKind = selGame.DefaultSortKind;

            // Load the folder, add it to history
            curFolder = selGame.MapsFolder;
            await UpdateMapList(selGame.MapsFolder);
            HistoryManager.AddToHistory(curFolder);
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
            sortMapsComboBox.Text = Sorting.Kinds[(int)MapInfoController.SortKind];

            mapListBox.Items.Refresh();

        }
        #endregion

        #region AdressBarButtonsEvents
        private async void refreshMapsButton_Click(object sender, RoutedEventArgs e)
        {
            await UpdateMapList(curFolder);
        }

        private void OpenInExplorerButton_Click(object sender, RoutedEventArgs e)
        {
            FileOperations.OpenInExplorer(curFolder);
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

        #region LaunchingItem
        private async Task MapListBoxLaunchItemAsync(FolderAndFileInfo item)
        {
            if (item is FolderInfo selFolder)
            {
                curFolder = selFolder.FullPath;
                HistoryManager.AddToHistory(curFolder);
                await UpdateMapList(curFolder);
                UpdateMapPreview(null);
            }
            else if (item is MapInfo mapInfo)
            {
                var selGame = GetSelectedGame();
                if (selGame == null) return;
                mapInfo.OpenMap(selGame);
            }
            else if (item is null)
                MessageBox.Show("Select a map to launch", "Impossible to load map", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private async Task LaunchItemsAsync(List<FolderAndFileInfo> items)
        {
            if (items.Count == 0) return; 
            if (items.Count == 1)
            {
                await MapListBoxLaunchItemAsync(items[0]);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Launch all maps?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;
                
            foreach (FolderAndFileInfo item in items)
            {
                if (item is MapInfo)
                    await MapListBoxLaunchItemAsync(item);
            }
        }
            
        private async void mapListBox_MouseDoubleClickAsync(object sender, MouseButtonEventArgs e)
        {
            await LaunchItemsAsync(selectedItems);
        }
        #endregion

        #region MapPreviewPane

        void UpdateMapPreviewVisibility(bool isVis)
        {
            if (isVis)
                mapPreviewColumn.Width = new GridLength(1, GridUnitType.Star);
            else
                mapPreviewColumn.Width = new GridLength(0, GridUnitType.Star);
        }

        void UpdateMapPreview(List<FolderAndFileInfo> data)
        {
            if (mapPreviewFrame.CanGoBack)
                mapPreviewFrame.RemoveBackEntry();
            mapPreviewFrame.Content = null;
            if (data == null) return;
            mapPreviewFrame.Content = new MapPreviewPage(data);
        }
        void MapPreview_SetPage(object page)
        {
            if (mapPreviewFrame.CanGoBack)
                mapPreviewFrame.RemoveBackEntry();
            mapPreviewFrame.Content = null;
            if (page == null) return;
            mapPreviewFrame.Content = (GbxInfoPage)page;
        }

        private void mapListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mapListBox.SelectedItem == null) return;
            selectedItems.Clear();
            foreach(FolderAndFileInfo selItem in mapListBox.SelectedItems)
            {
                selectedItems.Add(selItem);
            }
            UpdateMapPreview(selectedItems);
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
            await LaunchItemsAsync(selectedItems);
        }
        #endregion

        #region DragOutMaps

        void DragOutMaps(FolderAndFileInfo[] mapInfos)
        {
            List<string> files = new List<string>();
            Array.ForEach(mapInfos, mfo => files.Add(mfo.FullPath));

            var mapFile = new DataObject(DataFormats.FileDrop, files.ToArray());
            DragDrop.DoDragDrop((DependencyObject)mapListBox, mapFile, DragDropEffects.Copy);
        }

        private void mapListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            if (mapListBox.SelectedItems.Count == 0) return;
            if (e.MouseDevice.DirectlyOver is not TextBlock) return;

            string lastSelMapName = (e.MouseDevice.DirectlyOver as TextBlock).Text;
            List<string> mapNames = new List<string>();
            foreach (FolderAndFileInfo m in mapListBox.SelectedItems)
            {
                mapNames.Add(m.Name);
            }
            if (!mapNames.Contains(lastSelMapName)) return;

            if (MapInfoController.AtleastOneExists(selectedItems.ToArray()))
                DragOutMaps(selectedItems.ToArray());
        }

        #endregion

        #region DragInMaps
        private async void mapListBox_Drop(object sender, DragEventArgs e)
        {
            string[] paths = (string[])(e.Data).GetData(DataFormats.FileDrop, false);
            if (paths.Length == 0) return;
            try
            {
                FileOperations.CopyFilesToFolder(paths, curFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            await UpdateMapList(curFolder);

        }
        #endregion

        async Task DeleteSelectedItems()
        {
            // Delete all?
            if (selectedItems.Count > 1)
            {
                var result = MessageBox.Show("Are you sure to delete " + selectedItems.Count + " items?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                    foreach (FolderAndFileInfo item in selectedItems)
                    {
                        try
                        {
                            if (item is MapInfo)
                                await Task.Run(() => FileOperations.DeleteFile(item.FullPath));
                            else if (item is FolderInfo)
                                await Task.Run(() => Directory.Delete(item.FullPath, true));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                return;
            }

            //Delete One
            if (selectedItems[0] is FolderAndFileInfo itemInfo)
            {
                var messageBoxResult = MessageBox.Show($"Are you sure to delete {itemInfo.Name} \nPath: {itemInfo.FullPath}?", "Delete file?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    await Task.Run(() => FileOperations.DeleteFile(itemInfo.FullPath));

                }
            }
        }

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
                await LaunchItemsAsync(selectedItems);
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

            
            if (e.Key == Key.Delete)
            {
                await DeleteSelectedItems();
                await UpdateMapList(curFolder);
            }
            if (Keyboard.Modifiers == ModifierKeys.Shift && Keyboard.IsKeyDown(Key.F10))   //SHIFT + F10        
                ShowContextMenu(); //context menu
            if (e.SystemKey == Key.F2)
            {
                if(selectedItems.Count > 0)
                {
                    MessageBox.Show("Cannot rename multiple maps", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                MapOperations.RenameMap(selectedItems[0] as MapInfo);
                await UpdateMapList(curFolder);

            }
            if (Keyboard.IsKeyDown(Key.Enter) && Keyboard.Modifiers == ModifierKeys.Alt) //ALT + ENTER
                if(selectedItems.Count == 1)
                FileOperations.ShowFileProperties(selectedItems[0].FullPath);
        }

        #endregion

        #region ContextMenus

        void UpdateContextMenu()
        {
            if (selectedItems.Count == 0)
            {
                mapListBox.ContextMenu = (ContextMenu)FindResource("NoSelectionContextMenu");
                return;
            }

            if (selectedItems.Count == 1)
            {
                if (selectedItems[0] is MapInfo)
                    mapListBox.ContextMenu = (ContextMenu)FindResource("MapContextMenu");
                else if (selectedItems[0] is FolderInfo)
                    mapListBox.ContextMenu = (ContextMenu)FindResource("FolderContextMenu");
                return;
            }

            mapListBox.ContextMenu = (ContextMenu)FindResource("MultiselectionContextMenu");
        }

        void ShowContextMenu()
        {
            UpdateContextMenu();
            mapListBox.ContextMenu.PlacementTarget = mapListBox;
            mapListBox.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
            mapListBox.ContextMenu.IsOpen = true;
        }

        private void mapListBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            UpdateContextMenu();
            ((ListBox)sender).ContextMenu.PreviewMouseUp += ItemContextMenu_PreviewMouseUp;
        }

        private async void ItemContextMenu_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!(e.Source is MenuItem)) return;
            var selMenuItem = (MenuItem)e.Source;


            e.Handled = true; //avoid running this code more than once
            switch (selMenuItem.Header)
            {
                case "Open this folder in file explorer":
                    FileOperations.OpenInExplorer(curFolder);
                    break;
                case "Refresh":
                    await UpdateMapList(curFolder);
                    break;
                case "Paste":
                    try
                    {
                        string[] clipboardText = null;
                        await Task.Run(() =>
                        Dispatcher.Invoke(() =>
                        clipboardText = (string[])Clipboard.GetDataObject().GetData(DataFormats.FileDrop)
                        )
                        );

                        if (clipboardText is null)
                        {
                            throw new Exception("The clipboard is empty.");
                        }
                        else
                        {
                            FileOperations.CopyFilesToFolder(clipboardText, curFolder);
                            await UpdateMapList(curFolder);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                break;
                case "New Folder":
                    NewFolderWindow newFolderWindow = new NewFolderWindow(); 
                    newFolderWindow.ShowDialog();

                    if (string.IsNullOrEmpty(newFolderWindow.newName)) return;
                    Directory.CreateDirectory(curFolder + "\\" + newFolderWindow.newName);
                    break;
            }

            if (selectedItems.Count == 0)
            {
                await Task.Delay(100);
                ((ContextMenu)sender).IsOpen = false;
                return;
            }
            var selItem = selectedItems[0] as FolderAndFileInfo;
            string path = selItem.FullPath;

            switch (selMenuItem.Header)
            {
               
                case "Launch or open (all items)": 
                case "Launch": 
                case "Open": 
                    await LaunchItemsAsync(selectedItems);
                    break;
                case "Copy":
                    var fileDropList = new StringCollection();
                    fileDropList.Add(path);
                    Clipboard.SetFileDropList(fileDropList);
                    break;
                case "Delete":
                    await DeleteSelectedItems();
                    await UpdateMapList(curFolder);
                    break;
                case "Rename file":
                    var oldMapName = selItem.FullPath;
                    await Task.Run(() => FileOperations.RenameFile(oldMapName));
                    await UpdateMapList(curFolder);
                    break;
                case "Rename folder":
                    var oldName = selItem.FullPath;
                    await Task.Run(() => FileOperations.RenameFolder(oldName));
                    await UpdateMapList(curFolder);
                    break;
                case "Rename map":
                    MapOperations.RenameMap(selItem as MapInfo);
                    await UpdateMapList(curFolder);
                    break;
                case "File properties":
                case "Folder properties":
                    FileOperations.ShowFileProperties(path);
                    break;
                case "Properties (all items)":
                    string[] paths = (from item in selectedItems select item.FullPath).ToArray();
                    FileOperations.ShowFilesProperties(paths);
                    break;
                case "Map properties (GBX Preview)":
                    var gbxInfoPage = new GbxInfoPage(path);
                    MapPreview_SetPage(gbxInfoPage);
                    break;   
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

        #region Sorting
        private async void sortMapsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MapInfoController.SortKind = (Sorting.Kind)sortMapsComboBox.SelectedIndex;
            GbxGameController.SelectedGbxGame.DefaultSortKind = MapInfoController.SortKind;
            SettingsManager.SaveAllSettings(GbxGameController);
            await UpdateMapList(curFolder);
        }
        private void LoadSorting()
        {
            sortMapsComboBox.DataContext = MapInfoController;
            sortMapsComboBox.ItemsSource = Sorting.Kinds;
        }
        #endregion
    }
}
