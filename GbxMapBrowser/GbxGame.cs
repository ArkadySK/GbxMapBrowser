using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GbxMapBrowser
{
    public class GbxGame
    {
        public string Name { get; internal set; }
        public string InstalationFolder { get; set; }
        public string TargetExeName { get; internal set; }
        public string ExeLocation { get; internal set; }
        public string MapsFolder { get; set; }
        public bool IsVisibleInGameList { get; set; }

        public BitmapImage Icon { get; internal set; }
        public bool IsEnabled { get; set;}

        

        public GbxGame() { }

        public GbxGame(string name, string instalationfolder, string targetexename)
        {
            Name = name;
            Icon = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Data\\GameIcons\\" + Name + ".png"));
            TargetExeName = targetexename;
            ExeLocation = InstalationFolder + "\\" + ExeLocation;
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
            GetMapsFolder(InstalationFolder);
        }

        public void UpdateMapsFolder()
        {
            GetMapsFolder(InstalationFolder);
        }

        internal void GetMapsFolder(string instalationfolder)
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
                    else if (line.Contains("UserDir=")) //TMNext or MP
                    {
                        string foldername = line.Replace("UserDir=", "");
                        if (foldername.Contains("{userdocs}"))
                        {
                            foldername = foldername.Replace("{userdocs}", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                            MapsFolder = foldername + "\\Maps";
                        }
                        else if (foldername.Contains("{")) MessageBox.Show("Unknown mapdata folder");
                        else 
                        {
                            MapsFolder = foldername + "\\Maps";
                        }
                    }
                }
            }
            if (!Directory.Exists(MapsFolder))
            {
                if(this.Name == "TM 2020")
                {
                    MapsFolder = MapsFolder.Replace("\\Maps", "2020\\Maps");
                }
                else if (this.Name == "TM Turbo") //it's TMT, search for longest folder and then find maps subfolder
                {
                    string mainTMTFolder = MapsFolder.Replace("\\Maps", "");
                    if (!Directory.Exists(mainTMTFolder))
                    {
                        Console.WriteLine(this.Name + ": main folder not found!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    var allDirs = Directory.GetDirectories(mainTMTFolder);
                    var sortDirsByNamelendth = from dir in allDirs
                                               orderby dir.Length descending
                                               select dir;
                    Console.WriteLine(sortDirsByNamelendth.ToArray()[0]);
                    MapsFolder = sortDirsByNamelendth.ToArray()[0] + "\\MapsGhosts";
                }
                if (this.Name == "TM Nations Forever" || this.Name == "TM United Forever")
                {
                    MapsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TMForever\\Tracks\\Challenges";
                }

                if (!Directory.Exists(MapsFolder))
                MessageBox.Show("Error Game: " + this.Name + Environment.NewLine + " Folder '" + MapsFolder + "' not found!", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        public void GetInstallationDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Title = "Please locate the " + TargetExeName + " (game: " + Name + ")",
            };
            var dialogResult = openFileDialog.ShowDialog();
            if (!dialogResult.HasValue) return;

            if (dialogResult.Value)
            {
                ExeLocation = openFileDialog.FileName;
                var exeName = openFileDialog.FileName.Split("\\").Last();
                if (exeName == TargetExeName || TargetExeName == "")
                {
                    InstalationFolder = openFileDialog.FileName.Replace("\\" + exeName, "");
                    UpdateMapsFolder();
                    IsEnabled = true;
                }
                else
                    MessageBox.Show("Wrong exe file! '" + exeName + "' Executable names do not match.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
