using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace GbxMapBrowser
{
    internal class CustomGbxGame: GbxGame
    {
        public CustomGbxGame(string name, GbxGame baseGbxGame)
        {
            Name = name;
            Icon = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Data\\GameIcons\\Custom.png"));
            TargetExeName = "";
            IsVisibleInGameList = false;
            IsEnabled = true;
            MapsFolder = baseGbxGame.MapsFolder;
        }
    }
}
