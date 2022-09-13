using System;
using System.Collections.Generic;
using System.IO;
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

            try
            {
                // Fix for proper app CurrentDirectory value
                var appPath = Environment.CommandLine.Split(' ')[0];
                appPath = appPath.Replace(Path.GetFileName(appPath), "");
                Environment.CurrentDirectory = appPath;
            }
            catch   {   }

            var infos = new List<FolderAndFileInfo>();
            try
            {
                foreach (var path in e.Args)
                {
                    if (string.IsNullOrWhiteSpace(path))
                        continue;
                    if (Directory.Exists(path))
                        infos.Add(new FolderInfo(path));
                    else if (path.ToLower().EndsWith(".gbx"))
                        infos.Add(new MapInfo(path, true));
                }
                LaunchMapPreview(infos);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);    
            }
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
