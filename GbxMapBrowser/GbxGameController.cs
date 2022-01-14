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

        private void AddGbxGame(string name, string targetexename)
        {
            GbxGames.Add(new GbxGame(name, "", targetexename));
        }

        public void LoadGames()
        {
            GbxGames.Clear();
            AddGbxGame("TM Nations Forever", "TmForever.exe");
            AddGbxGame("TM United Forever", "TmForever.exe");
            AddGbxGame("ManiaPlanet", "ManiaPlanet.exe");
            AddGbxGame("TM Turbo", "TrackmaniaTurbo.exe");
            AddGbxGame("TM 2020", "Trackmania.exe");
            UpdateSettingsFromFile();
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

            int emptyGamesCount = 0;
            foreach(var line in settingsText)
            {
                foreach(GbxGame game in GbxGames)
                {
                    if (line.Contains(game.Name)) //game is found
                    {
                        string mapPath = line.Replace(game.Name + ": ", "");
                        if(mapPath == "")
                        {
                            emptyGamesCount++;
                            game.IsVisibleInGameList = false;
                            game.IsEnabled = true;

                        }
                        else
                        {
                            game.MapsFolder = mapPath;
                            game.IsVisibleInGameList = true;
                            game.IsEnabled = true;
                        }
                    }

                }
            }

            if(emptyGamesCount == GbxGames.Count)
                Properties.Settings.Default.IsFirstRun = true;

        }
    }
}
