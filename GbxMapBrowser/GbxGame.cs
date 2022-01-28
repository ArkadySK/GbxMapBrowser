using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GbxMapBrowser
{
    public class GbxGame : INotifyPropertyChanged
    {
        private string _exeLocation;
        private bool _isVisibleInGameList;

        public event PropertyChangedEventHandler PropertyChanged;

        internal void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        public string Name { get; set; }
        public string ExeLocation
        {
            get => _exeLocation;
            internal set
            {
                _exeLocation = value;
                NotifyPropertyChanged();
            }
        }
        public string InstalationFolder
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ExeLocation))
                    return null;
                var curExeName = @"\" + ExeLocation.Split("\\").Last();
                string parentFolder = ExeLocation.Replace(curExeName, "");
                return parentFolder;
            }
        }
        public string TargetExeName { get; internal set; }
        public string MapsFolder { get; set; }
        public bool IsVisibleInGameList
        {
            get => _isVisibleInGameList;
            set
            {
                _isVisibleInGameList = value;
                NotifyPropertyChanged();
            }
        }

        public BitmapImage Icon { get; internal set; }
        public bool IsVisibleInGameLaunchMenu { get; set; }



        public GbxGame() { }

        public GbxGame(string name, string instalationfolder, string targetexename)
        {
            Name = name;
            Icon = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Data\\GameIcons\\" + Name + ".png"));
            TargetExeName = targetexename;
            if (Directory.Exists(instalationfolder))
            {
                IsVisibleInGameLaunchMenu = true;
                IsVisibleInGameList = true;
            }
            else
            {
                IsVisibleInGameLaunchMenu = false;
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
                if (this.Name == "TM 2020")
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

        public void GetInstallationAndMapFolderDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Title = "Please locate the " + TargetExeName + " (game: " + Name + ")",
            };
            if (ExeLocation != null)
                openFileDialog.InitialDirectory = InstalationFolder;

            var dialogResult = openFileDialog.ShowDialog();
            if (!dialogResult.HasValue) return;

            if (dialogResult.Value)
            {
                var exeName = openFileDialog.FileName.Split("\\").Last();
                if (exeName == TargetExeName || TargetExeName == "")
                {
                    ExeLocation = openFileDialog.FileName;
                    UpdateMapsFolder();
                    IsVisibleInGameLaunchMenu = true;
                    IsVisibleInGameList = true;
                }
                else
                    MessageBox.Show("Wrong exe file! '" + exeName + "' Executable names do not match.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Launch()
        {
            var gameProcess = new Process();
            gameProcess.StartInfo.FileName = ExeLocation;
            gameProcess.Start();
        }
    }
}
