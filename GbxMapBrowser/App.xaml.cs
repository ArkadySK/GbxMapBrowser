using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace GbxMapBrowser
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 0)
                return;

            var infos = new List<FolderAndFileInfo>();
            foreach(var file in e.Args)
            {
                if (file.EndsWith(".Gbx"))
                    infos.Add(new MapInfo(file, true));
                else
                    infos.Add(new FolderInfo(file));
            }

            LaunchMapPreview(infos);
        }

        private void LaunchMapPreview(List<FolderAndFileInfo> infos)
        {
            Window window = new Window();
            var previewPage = new MapPreviewPage(infos);
            window.SizeToContent = SizeToContent.WidthAndHeight;
            previewPage.mapImage.Stretch = System.Windows.Media.Stretch.None;
            window.Content = previewPage;
            window.ShowDialog();
        }
    }
}
