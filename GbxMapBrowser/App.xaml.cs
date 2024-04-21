using GBX.NET;
using GBX.NET.LZO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;

namespace GbxMapBrowser
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Gbx.LZO = new MiniLZO();

            if (e.Args.Length == 0)
                return;

            try
            {
                // Fix for proper app CurrentDirectory value
                Environment.CurrentDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

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
