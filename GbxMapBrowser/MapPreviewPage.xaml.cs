using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace GbxMapBrowser
{
    /// <summary>
    /// Interaction logic for MapPreviewPage.xaml
    /// </summary>
    public partial class MapPreviewPage : Page
    {
        List<FolderAndFileInfo> Data;

        public MapPreviewPage(List<FolderAndFileInfo> data)
        {       
            InitializeComponent();
            Data = data;
            Opacity = 0;
            if(data.Count == 0)
            {
                HideMapPreviewUI();
                return;
            }
            if (data.Count > 1)
            {
                mapNameLabel.Content = "Selected " + data.Count + " items";
                descriptionTextBlock.Text = "Number of maps: " + data.FindAll(x => x is MapInfo).Count.ToString();
                HideMapPreviewUI();
                return;
            }

        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Data.Count != 1)
                return;
            var item = Data[0];

            if (item is null)
                return;

            if (item is MapInfo map)
            {
                var fullMap = await Task.Run(() => new MapInfo(map.FullPath, false));
                if (!fullMap.IsWorking) HideMapPreviewUI();
                else
                {
                    mapImage.ContextMenu = App.Current.Resources["ThumbnailContextMenu"] as ContextMenu;
                    mapImage.ContextMenu.PreviewMouseUp += MapThumbnailContextMenu_PreviewMouseUp;
                }
                Dispatcher.Invoke(() =>
                {
                    if (Data is not null)
                        Data[0] = fullMap;
                    DataContext = fullMap;
                });        
            }
            else if (item is FolderInfo folder)
            {
                DataContext = folder;
                await Task.Run(
                    () => Dispatcher.BeginInvoke(
                        () => PreviewFolder(folder)
                    )
                );
            }
            FadeInAnimation();
        }
             
        void PreviewFolder(FolderInfo folderInfo)
        {
            HideMapPreviewUI();
            if (folderInfo == null) return;
            mapImage.Source = new BitmapImage(folderInfo.ImageSmall);
            mapNameLabel.Content = folderInfo.DisplayName;
            descriptionTextBlock.Text = 
                "Contains: " + Environment.NewLine
                + "Files: " + folderInfo.FilesInsideCount + Environment.NewLine
                + "Maps: " + folderInfo.MapsInsideCount;
            
        }

        void FadeInAnimation()
        {
            DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.3));
            BeginAnimation(OpacityProperty, animation);
        }

        void HideMapPreviewUI()
        {
            mapInfoExpander.Visibility = Visibility.Collapsed;
            medalsViewBox.Visibility = Visibility.Collapsed;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Data = null;
            GC.Collect();
        }

        #region Context menu
        private async void MapThumbnailContextMenu_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!(e.Source is MenuItem)) return;
            var selMenuItem = (MenuItem)e.Source;

            if (Data == null) return;

            var mapInfo = Data[0] as MapInfo;
            if (mapInfo == null) return;

            string curPath = Environment.CurrentDirectory;

            e.Handled = true; //avoid running this code more than once
            switch (selMenuItem.Header)
            {
                case "Open image":
                    string path = curPath + "\\Temp\\" + mapInfo.OriginalName + ".png";
                    var task = mapInfo.ExportThumbnail(path);
                    await task;
                    if (task.IsCompleted)
                        ProcessManager.OpenFile(path);
                    else
                        MessageBox.Show("Error exporting thumbnail", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case "Export as image":
                    string path2 = curPath + "\\Thumbnails\\" + mapInfo.OriginalName + ".png";
                    var task2 = mapInfo.ExportThumbnail(path2);
                    await task2;
                    if (task2.IsCompleted)
                        MessageBox.Show("Thumbnail exported successfully!\n\nPath: " + path2, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    else
                        MessageBox.Show("Error exporting thumbnail", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    break;
            }
        }
        #endregion


    }
}
