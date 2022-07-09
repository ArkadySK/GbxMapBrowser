using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GbxMapBrowser
{
    internal class CustomGbxGame: GbxGame
    {
        private bool _isUnlimiter;
        public bool IsUnlimiter
        {
            get
                => _isUnlimiter;
            set {
                base.NotifyPropertyChanged();
                _isUnlimiter = value;
            }
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
        public void SetCustomExe()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = ".exe|*.exe",
                Title = "Select the .exe file for " + Name
            };

            if (!string.IsNullOrWhiteSpace(InstalationFolder))
                dialog.InitialDirectory = Path.GetFileName(InstalationFolder);

            bool? dialogResult = dialog.ShowDialog();
            if(!dialogResult.HasValue)
                return;
            if(dialogResult.Value == true)
                ExeLocation = dialog.FileName;
        }

        public void SetCustomIcon()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Title = "Select the icon of " + Name,
                Filter = "Supported Image Files (*.png, *.jpg, *.bmp, *.ico)|*.png;*.jpg;*.bmp;*.ico|All files (*.*)|*.*"
        };
            bool? dialogResult = dialog.ShowDialog();
            if (!dialogResult.HasValue)
                return;
            if(dialogResult == true)
                Icon = new BitmapImage(new Uri(dialog.FileName));
        }
    }
}
