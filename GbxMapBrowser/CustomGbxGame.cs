using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace GbxMapBrowser
{
    internal class CustomGbxGame: GbxGame
    {
        public CustomGbxGame(string name, string instalationfolder)
        {
            Name = name;
            Icon = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Data\\GameIcons\\Custom.png"));
            TargetExeName = "";
            if (Directory.Exists(instalationfolder))
            {
                InstalationFolder = instalationfolder;
                IsEnabled = true;
                IsVisibleInGameList = true;
            }
            else
            {
                IsEnabled = false;
                return;
            }
            base.GetMapsFolder(InstalationFolder);
        }
    }
}
