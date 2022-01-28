using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
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
            MapsFolder = baseGbxGame.MapsFolder;
            ExeLocation = baseGbxGame.ExeLocation;
            IsVisibleInGameLaunchMenu = baseGbxGame.IsVisibleInGameLaunchMenu;
            IsVisibleInGameList = baseGbxGame.IsVisibleInGameList;
        }

        public CustomGbxGame()
        {

        }

        public void SetCustomMapsFolder()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();

            if(!string.IsNullOrWhiteSpace(MapsFolder))
                dialog.InitialDirectory = MapsFolder; 
            dialog.Title = "Select a Maps Folder for " + Name; 
            dialog.Filter = "Directory|*.this.directory"; // prevents displaying files
            dialog.FileName = "select";

            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                path = path.Replace("\\select.this.directory", "");
                path = path.Replace(".this.directory", "");
                // If user has changed the filename, create the new directory
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                // Our final value is in path
                MapsFolder = path;
            }        
        }
    }
}
