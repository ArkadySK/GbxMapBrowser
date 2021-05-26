using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GbxMapBrowser
{
    public class GbxGame
    {
        public string Name { get;}
        public string InstalationFolder { get; set; }
        public string TargetExeName { get; }
        public string MapsFolder { get; set; }
        public BitmapImage Icon { get; }
        private bool isEnabled = false;
        public bool IsEnabled { get { return isEnabled; }  set 
            {
                if (value == true)
                {
                    Visibility = Visibility.Visible;
                }
                else
                    Visibility = Visibility.Collapsed;
                isEnabled = value;
            } 
        }
        public Visibility Visibility { get; set; }

        

        public GbxGame() { }

        public GbxGame(string name, string instalationfolder, string targetexename)
        {
            Name = name;
            Icon = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Data\\GameIcons\\" + Name + ".png"));
            TargetExeName = targetexename;
            if (Directory.Exists(instalationfolder))
            {
                InstalationFolder = instalationfolder;
                IsEnabled = true;
            }
            else
            {
                IsEnabled = false;
                return;
            }
            GetMapsFolder(InstalationFolder);
        }

        public void UpdateMapsFolder()
        {
            GetMapsFolder(InstalationFolder);
        }

        void GetMapsFolder(string instalationfolder)
        {
            string nadeoiniPath = instalationfolder + "\\Nadeo.ini";
            if (!File.Exists(nadeoiniPath))
            {
                MessageBox.Show("You have picked wrong folder or it does not contain nadeo.ini!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var nadeoini = File.ReadAllLines(nadeoiniPath);
            foreach (string line in nadeoini)
            {
                if (line.Contains("User"))
                {
                    if (line.Contains("UserSubDir")) //it's TMF
                    {
                        string foldername = line.Replace("UserSubDir=", "");
                        MapsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + foldername + "\\Tracks\\Challenges";
                    }
                    else if (line.Contains("UserDir=")) //TMNext alebo MP
                    {
                        string foldername = line.Replace("UserDir=", "");
                        if (foldername.Contains("{userdocs}"))
                        {
                            foldername = foldername.Replace("{userdocs}", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                            MapsFolder = foldername + "\\Maps";
                        }
                        else if (foldername.Contains("{")) MessageBox.Show("Unknown mapdata folder");
                        else MapsFolder = foldername + "\\Maps";
                    }
                }
            }
            if (!Directory.Exists(MapsFolder))
            {
                if(this.TargetExeName == "Trackmania.exe")
                {
                    MapsFolder = MapsFolder.Replace("\\Maps", "2020\\Maps");
                }
                
                if(!Directory.Exists(MapsFolder))
                MessageBox.Show("Folder '" + MapsFolder + "' not found!", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}
