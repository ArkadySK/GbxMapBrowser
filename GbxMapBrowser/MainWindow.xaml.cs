using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GbxMapBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string _curFolder = "";
        private readonly MapInfoViewModel _mapInfoViewModel = new();
        private readonly GbxGameViewModel _gbxGameViewModel = new();
        private readonly SearchOption _searchOption;
        private readonly List<FolderAndFileInfo> _selectedItems = [];

        #region Initialization
        public MainWindow()
        {
            _curFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _searchOption = SearchOption.TopDirectoryOnly;
            InitializeComponent();
            LoadGbxGameList();
            UpdateMapPreviewVisibility(Properties.Settings.Default.ShowMapPreviewColumn);
            loadingLabel.DataContext = _mapInfoViewModel;
            LoadSorting();
            //Properties.Settings.Default.IsFirstRun = true;
            if (Properties.Settings.Default.IsFirstRun)
            {
                ShowGbxGamesWindow();
            }
        }

        private async void Window_LoadedAsync(object sender, RoutedEventArgs e)
        {
            await UpdateMapList(_curFolder);

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
                if (result == MessageBoxResult.Yes)
                {
                    SettingsManager.SaveAllSettings(_gbxGameViewModel);
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
            gamesListMenu.DataContext = _gbxGameViewModel;
            openInComboBox.DataContext = _gbxGameViewModel;
            _gbxGameViewModel.LoadGames();
        }

        void ShowGbxGamesWindow()
        {
            SettingsWindow settingsWindow = new SettingsWindow(_gbxGameViewModel);
            if (this.IsVisible && this is not null)
                settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }
        #endregion

        #region GbxGameListMethods
        private void ManageGamesButton_Click(object sender, RoutedEventArgs e)
        {
            ShowGbxGamesWindow();
        }

        private async void GamesListMenu_ItemClick(object sender, MahApps.Metro.Controls.ItemClickEventArgs args)
        {
            if (gamesListMenu.SelectedItem == null) return;

            var selGame = (GbxGame)gamesListMenu.SelectedItem;
            if (!selGame.IsVisibleInGameList) return;

            // Assign selection of the game
            _gbxGameViewModel.SelectedGbxGame = selGame;
            if (selGame.IsVisibleInGameLaunchMenu)
                openInComboBox.SelectedItem = selGame;
            else
                openInComboBox.SelectedItem = null;
            await Task.Delay(100);

            // Assign sorting
            _mapInfoViewModel.SortKind = selGame.DefaultSortKind;

            // Load the folder, add it to history
            _curFolder = selGame.MapsFolder;

            await UpdateMapList(selGame.MapsFolder);
            HistoryManager.AddToHistory(_curFolder);
            await Task.CompletedTask;
        }
        #endregion

        #region GetDataAndUpdateUI

        private async Task UpdateMapList(string mapsFolder)
        {
            _mapInfoViewModel.IsLoading = true;

            UpdateMapPreview(null);
            _mapInfoViewModel.ClearMapList();

            //update enabled/disabled navigation buttons
            undoButton.IsEnabled = HistoryManager.CanUndo;
            redoButton.IsEnabled = HistoryManager.CanRedo;


            Application.Current.Dispatcher.Invoke(() =>
            {
                mapListBox.ItemsSource = null;
                currentFolderTextBox.Text = mapsFolder;
            }
            );
            string[] folders = await Task.Run(() => FileOperations.TryGetFolders(mapsFolder));
            var mapFiles = await Task.Run(() => FileOperations.TryGetMapPaths(mapsFolder, _searchOption));
            int i = 0;

            foreach (var folderPath in folders)
            {
                await _mapInfoViewModel.AddFolder(folderPath);
                i++;
            }
            foreach (string mapPath in mapFiles)
            {
                await _mapInfoViewModel.AddMap(mapPath);
                i++;
            }

            await _mapInfoViewModel.SortMapList();

            mapListBox.ItemsSource = _mapInfoViewModel.MapList;
            sortMapsComboBox.Text = Sorting.Kinds[(int)_mapInfoViewModel.SortKind];

            mapListBox.Items.Refresh();
            _mapInfoViewModel.IsLoading = false;


        }
        #endregion

        #region AdressBarButtonsEvents
        private async void refreshMapsButton_Click(object sender, RoutedEventArgs e)
        {
            await UpdateMapList(_curFolder);
        }

        private void OpenInExplorerButton_Click(object sender, RoutedEventArgs e)
        {
            FileOperations.OpenInExplorer(_curFolder);
        }

        private async void undoButton_Click(object sender, RoutedEventArgs e)
        {
            _curFolder = await Task.Run(HistoryManager.RequestPrev);
            await UpdateMapList(_curFolder);
        }

        private async void redoButton_Click(object sender, RoutedEventArgs e)
        {
            _curFolder = await Task.Run(HistoryManager.RequestNext);
            await UpdateMapList(_curFolder);
        }

        private async void parentFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var parentFolder = (Directory.GetParent(_curFolder));
                if (parentFolder != null)
                    _curFolder = parentFolder.FullName;
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
            await UpdateMapList(_curFolder);
            HistoryManager.AddToHistory(_curFolder);

        }
        #endregion

        #region LaunchingItemAndMapListSelection
        private async Task MapListBoxLaunchItemAsync(FolderAndFileInfo item)
        {
            if (_mapInfoViewModel.IsLoading) return;
            if (item is FolderInfo selFolder)
            {
                _curFolder = selFolder.FullPath;
                HistoryManager.AddToHistory(_curFolder);
                await UpdateMapList(_curFolder);
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
            await LaunchItemsAsync(_selectedItems);
        }
        private void mapListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedItems.Clear();
            if (mapListBox.SelectedItem == null) return;
            foreach (FolderAndFileInfo selItem in mapListBox.SelectedItems)
            {
                _selectedItems.Add(selItem);
            }
            UpdateMapPreview(_selectedItems);
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

        private GbxGame GetSelectedGame()
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
            await LaunchItemsAsync(_selectedItems);
        }
        #endregion

        #region DragInOutMaps

        Point initialMousePosition;
        private void DragOutMaps(FolderAndFileInfo[] mapInfos)
        {
            List<string> files = [];
            Array.ForEach(mapInfos, mfo => files.Add(mfo.FullPath));

            var mapFile = new DataObject(DataFormats.FileDrop, files.ToArray());
            DragDrop.DoDragDrop((DependencyObject)mapListBox, mapFile, DragDropEffects.Copy);
        }

        private bool IsSelectedItemByName(string name)
        {
            foreach (var mapInfo in _selectedItems)
            {
                if (mapInfo.DisplayName == name) return true;
            }
            return false;
        }

        private void MapListBox_PreviewMouseleftButonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2) return;
            if (_selectedItems.Count == 0) return;

            initialMousePosition = e.GetPosition(mapListBox);
        }

        private void MapListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            if (e.OriginalSource is MahApps.Metro.Controls.MetroThumb)
                return;

            // To avoid unintentional drag
            Point mousePosition = e.GetPosition(mapListBox);
            Vector diff = initialMousePosition - mousePosition;

            if (Math.Abs(diff.X) < 10 && Math.Abs(diff.Y) < 10)
                return;

            bool canCopy = false;
            e.Handled = true;
            // Check if it is not dragging out non-selected item
            if (e.OriginalSource is Border border)
            {
                if (border.Child is ContentPresenter contentPresenter)
                {
                    var mapName = contentPresenter.Content.ToString();
                    canCopy = IsSelectedItemByName(mapName);
                }
            }
            else if (e.OriginalSource is TextBlock textBlock)
            {
                canCopy = IsSelectedItemByName(textBlock.Text);
            }

            if (!canCopy) return;

            if (_mapInfoViewModel.IsLoading) return;
            DragOutMaps(_selectedItems.ToArray());
        }

        private async void MapListBox_Drop(object sender, DragEventArgs e)
        {
            if (_mapInfoViewModel.IsLoading) return;
            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (paths.Length == 0) return;
            try
            {
                FileOperations.CopyFilesToFolder(paths, _curFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            await UpdateMapList(_curFolder);

        }
        #endregion

        #region ItemOperations
        private async Task DeleteItem(FolderAndFileInfo item)
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

        private async Task DeleteSelectedItems()
        {
            // Delete all?
            if (_selectedItems.Count > 1)
            {
                var result = MessageBox.Show("Are you sure to delete " + _selectedItems.Count + " items?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                    foreach (FolderAndFileInfo item in _selectedItems)
                    {
                        await DeleteItem(item);
                    }
                return;
            }

            //Delete One
            if (_selectedItems[0] is FolderAndFileInfo itemInfo)
            {
                var messageBoxResult = MessageBox.Show($"Are you sure to delete {itemInfo.DisplayName} \nPath: {itemInfo.FullPath}?", "Delete file?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    await DeleteItem(itemInfo);
                }
            }
        }
        #endregion

        #region CopyAndPaste
        private void CopyItemsToMemory(List<FolderAndFileInfo> selectedItems)
        {
            if (selectedItems.Count == 0)
                return;
            var fileDropList = new StringCollection();
            foreach (var item in selectedItems)
                fileDropList.Add(item.FullPath);
            Clipboard.SetFileDropList(fileDropList);
        }

        private async Task PasteItemsFromMemory()
        {
            string[] clipboardText = null;
            await Task.Run(() =>
            Dispatcher.Invoke(() =>
                clipboardText = (string[])Clipboard.GetDataObject().GetData(DataFormats.FileDrop)
            ));

            if (clipboardText is null)
            {
                throw new Exception("The clipboard is empty.");
            }
            else
            {
                FileOperations.CopyFilesToFolder(clipboardText, _curFolder);
                await UpdateMapList(_curFolder);
            }
        }
        #endregion

        #region KeyPresses

        private async void CurrentFolderTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _curFolder = currentFolderTextBox.Text;
                await UpdateMapList(_curFolder);
                HistoryManager.AddToHistory(_curFolder);
            }
        }

        private async void MapListBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Launch / Open
            if (e.Key == Key.Enter)
            {
                await LaunchItemsAsync(_selectedItems);
            }

            // Parent directory
            if (e.Key == Key.Back)
            {
                try
                {
                    var parentFolder = Directory.GetParent(_curFolder);
                    if (parentFolder != null)
                        _curFolder = parentFolder.FullName;
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.Message);
                }
                await UpdateMapList(_curFolder);
                HistoryManager.AddToHistory(_curFolder);
            }

            // Undo
            if (Keyboard.Modifiers == ModifierKeys.Alt && Keyboard.IsKeyDown(Key.Left))
                if (HistoryManager.CanUndo)
                    undoButton_Click(this, null);

            // Redo
            if (Keyboard.Modifiers == ModifierKeys.Alt && Keyboard.IsKeyDown(Key.Right))
                if (HistoryManager.CanRedo)
                    redoButton_Click(this, null);

            // Delete
            if (e.Key == Key.Delete)
            {
                if (_selectedItems.Count == 0)
                    return;
                await DeleteSelectedItems();
                await UpdateMapList(_curFolder);
            }

            // Show context menu
            if (Keyboard.Modifiers == ModifierKeys.Shift && Keyboard.IsKeyDown(Key.F10))   //SHIFT + F10        
                ShowContextMenu();

            // Rename
            if (e.Key == Key.F2)
            {
                if (_selectedItems.Count == 0)
                    return;
                if (_selectedItems.Count > 1)
                {
                    MessageBox.Show("Cannot rename multiple maps", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    if (_selectedItems[0] is MapInfo mapInfo)
                    {
                        MapOperations.RenameMap(mapInfo);
                        await UpdateMapList(_curFolder);
                    }
                    return;
                }
                if (_selectedItems[0] is MapInfo)
                    FileOperations.RenameFile(_selectedItems[0].FullPath);
                else if (_selectedItems[0] is FolderInfo)
                    FileOperations.RenameFolder(_selectedItems[0].FullPath);
                await UpdateMapList(_curFolder);


            }

            // Properties
            if (Keyboard.IsKeyDown(Key.Enter) && Keyboard.Modifiers == ModifierKeys.Alt) //ALT + ENTER
                if (_selectedItems.Count == 1)
                    FileOperations.ShowFileProperties(_selectedItems[0].FullPath);

            // Refresh
            if (e.Key == Key.F5)
            {
                refreshMapsButton_Click(null, null);
            }

            // Copy
            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
            {
                CopyItemsToMemory(_selectedItems);
            }

            // Paste
            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                try
                {
                    await PasteItemsFromMemory();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region ContextMenus

        void UpdateContextMenu()
        {
            if (_selectedItems.Count == 0)
            {
                mapListBox.ContextMenu = (ContextMenu)FindResource("NoSelectionContextMenu");
                return;
            }

            if (_selectedItems.Count == 1)
            {
                if (_selectedItems[0] is MapInfo)
                    mapListBox.ContextMenu = (ContextMenu)FindResource("MapContextMenu");
                else if (_selectedItems[0] is FolderInfo)
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

        private void MapListBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (e.OriginalSource is ScrollViewer)
                mapListBox.SelectedItem = null;
            UpdateContextMenu();
            ((ListBox)sender).ContextMenu.PreviewMouseUp += ItemContextMenu_PreviewMouseUp;
        }

        private async void ItemContextMenu_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is not MenuItem) return;
            var selMenuItem = (MenuItem)e.Source;


            e.Handled = true; //avoid running this code more than once
            switch (selMenuItem.Header)
            {
                case "Open this folder in file explorer":
                    FileOperations.OpenInExplorer(_curFolder);
                    break;
                case "Refresh":
                    await UpdateMapList(_curFolder);
                    break;
                case "Paste":
                    try
                    {
                        await PasteItemsFromMemory();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
                case "New folder":
                    NewFolderWindow newFolderWindow = new NewFolderWindow();
                    newFolderWindow.ShowDialog();

                    if (string.IsNullOrEmpty(newFolderWindow.newName)) return;
                    Directory.CreateDirectory(_curFolder + "\\" + newFolderWindow.newName);
                    await UpdateMapList(_curFolder);
                    break;
            }

            if (_selectedItems.Count == 0)
            {
                await Task.Delay(100);
                ((ContextMenu)sender).IsOpen = false;
                return;
            }
            var selItem = _selectedItems[0] as FolderAndFileInfo;
            string path = selItem.FullPath;

            switch (selMenuItem.Header)
            {

                case "Launch or open (all items)":
                case "Launch":
                case "Open":
                    await LaunchItemsAsync(_selectedItems);
                    break;
                case "Copy":
                    CopyItemsToMemory(_selectedItems);
                    break;
                case "Delete":
                    await DeleteSelectedItems();
                    await UpdateMapList(_curFolder);
                    break;
                case "Rename file":
                    var oldMapName = selItem.FullPath;
                    FileOperations.RenameFile(oldMapName);
                    await UpdateMapList(_curFolder);
                    break;
                case "Rename folder":
                    var oldName = selItem.FullPath;
                    FileOperations.RenameFolder(oldName);
                    await UpdateMapList(_curFolder);
                    break;
                case "Rename map":
                    MapOperations.RenameMap(selItem as MapInfo);
                    await UpdateMapList(_curFolder);
                    break;
                case "File properties":
                case "Folder properties":
                    FileOperations.ShowFileProperties(path);
                    break;
                case "Properties (all items)":
                    string[] paths = (from item in _selectedItems select item.FullPath).ToArray();
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

        private void GameLibraryItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
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
                    await Task.Run(() => SettingsManager.SaveAllSettings(_gbxGameViewModel));
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

        private async void SearchMapsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            await Task.Delay(200);
            string text = searchMapsTextBox.Text;
            if (text == "search for a map...")
                return;
            if (string.IsNullOrEmpty(text))
            {
                await UpdateMapList(_curFolder);
                return;
            }

            _selectedItems.Clear();
            mapListBox.SelectedItems.Clear();
            await _mapInfoViewModel.FindMaps(text);
            mapListBox.Items.Refresh();
        }

        private void SearchMapsTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchMapsTextBox.Text == "search for a map...")
                searchMapsTextBox.Text = "";
            searchMapsTextBox.Opacity = .9;
        }

        private void SearchMapsTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Set the color back
            searchMapsTextBox.Opacity = .5;

            // Restore default view if searchbox is empty
            if (!string.IsNullOrWhiteSpace(searchMapsTextBox.Text) || searchMapsTextBox.Text == "search for a map...")
                return;
            searchMapsTextBox.Text = "search for a map...";
        }
        #endregion

        #region Sorting
        private async void SortMapsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _mapInfoViewModel.SortKind = (Sorting.Kind)sortMapsComboBox.SelectedIndex;
            _gbxGameViewModel.SelectedGbxGame.DefaultSortKind = _mapInfoViewModel.SortKind;
            SettingsManager.SaveAllSettings(_gbxGameViewModel);
            await UpdateMapList(_curFolder);
        }
        private void LoadSorting()
        {
            sortMapsComboBox.DataContext = _mapInfoViewModel;
            sortMapsComboBox.ItemsSource = Sorting.Kinds;
        }
        #endregion

    }
}
