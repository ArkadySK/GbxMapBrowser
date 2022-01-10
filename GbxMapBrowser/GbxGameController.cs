using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace GbxMapBrowser
{
    public class GbxGameController
    {
        public ObservableCollection<GbxGame> GbxGames { get; private set; } = new ObservableCollection<GbxGame>();
        public GbxGame SelectedGbxGame { get; set; } = new GbxGame();

        private void AddGbxGame(string name, string instalationfolder, string targetexename)
        {
            GbxGames.Add(new GbxGame(name, instalationfolder, targetexename));
        }

        public void LoadGames()
        {
            GbxGames.Clear();
            UpdateSettingsFromFile();
            AddGbxGame("TM Nations Forever", Properties.Settings.Default.TMNationsForeverFolder, "TmForever.exe");
            AddGbxGame("TM United Forever", Properties.Settings.Default.TMUnitedForeverFolder, "TmForever.exe");
            AddGbxGame("ManiaPlanet", Properties.Settings.Default.ManiaPlanetFolder, "ManiaPlanet.exe");
            AddGbxGame("TM Turbo", Properties.Settings.Default.TMTurboFolder, "TrackmaniaTurbo.exe");
            AddGbxGame("TM 2020", Properties.Settings.Default.TMNextFolder, "Trackmania.exe");
        }

        public void SaveSettings()
        {
            string currentPath = Directory.GetCurrentDirectory();
            if (!Directory.Exists(currentPath + "\\config"))
                Directory.CreateDirectory(currentPath + "\\config");

            List<string> settingsText = new List<string>();
            foreach(GbxGame game in GbxGames)
            {
                settingsText.Add(game.Name + ": " + game.MapsFolder);
            }
            File.WriteAllLinesAsync(currentPath + "\\config\\settings.dat", settingsText);
        }

        void UpdateSettingsFromFile()
        {
            string currentPath = Directory.GetCurrentDirectory();
            if (!Directory.Exists(currentPath + "\\config"))
                Directory.CreateDirectory(currentPath + "\\config");
            if (!File.Exists(currentPath + "\\config\\settings.dat"))
            {
                Properties.Settings.Default.Reset();
                Properties.Settings.Default.IsFirstRun = true;
                return;
            }

            var settingsText = File.ReadAllLines(currentPath + "\\config\\settings.dat");

            foreach(var line in settingsText)
            {
                foreach(GbxGame game in GbxGames)
                {
                    if (line.Contains(game.Name)) //game is found
                    {
                        int curIndex = settingsText.ToList().IndexOf(line);
                        if (curIndex >= settingsText.Length) continue;

                        string mapPath = settingsText[curIndex + 1].Replace(game.Name + ": ", "");
                        game.MapsFolder = mapPath;
                    }

                }
            }
        }
    }
}
