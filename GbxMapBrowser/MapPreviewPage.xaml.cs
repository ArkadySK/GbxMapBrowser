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
        MapInfo Map;

        public MapPreviewPage(object data)
        {       
            InitializeComponent();
            Opacity = 0;
            if (data is MapInfo)
            {
                Map = (MapInfo)data;   
            }
            else if (data is FolderInfo)
            {
                mapNameLabel.Content = (data as FolderInfo).Name;
                HideAllExceptHeader();
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(new TimeSpan(50000));
            if (Map != null) 
            {
                Map = await Task.Run(() => new MapInfo(Map.FullPath, false));
                DataContext = Map;
                if (!Map.IsWorking) HideAllExceptHeader();
            }
            FadeInAnimation();
        }

        void FadeInAnimation()
        {
            DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.3));
            BeginAnimation(OpacityProperty, animation);
        }

        void HideAllExceptHeader()
        {
            infoStackPanel.Visibility = Visibility.Collapsed;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Map = null;
            GC.Collect();
        }
    }
}
