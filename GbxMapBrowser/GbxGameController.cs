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
                if (game is CustomGbxGame)
                    continue;
                string enabledString = "N";
                if (game.IsVisibleInGameLaunchMenu)
                    enabledString = "E";
                string visibleInGameListString = "N";
                if (game.IsVisibleInGameList)
                    visibleInGameListString = "V";

                settingsText.Add(game.Name + ": " + game.MapsFolder + "|" + game.ExeLocation  + "|" + enabledString + "|" + visibleInGameListString);
            }
            File.WriteAllLinesAsync(currentPath + "\\config\\settings.dat", settingsText);
        }

        public void UpdateSettingsFromFile()
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
                        string newline = line.Replace(game.Name + ": ", "");
                        string[] props = newline.Split('|');
                        string mapPath = props[0];
                        string exePath = props[1];

                        if (string.IsNullOrWhiteSpace(mapPath) || string.IsNullOrWhiteSpace(exePath))
                        {
                            emptyGamesCount++;
                            game.IsVisibleInGameList = false;
                            game.IsVisibleInGameLaunchMenu = false;
                            continue;
                        }

                        game.MapsFolder = mapPath;
                        game.ExeLocation = exePath;

                        if(props.Length >=3)
                        {
                            string enabledString = props[2];
                            if (enabledString == "E")
                                game.IsVisibleInGameLaunchMenu = true;
                            else
                                game.IsVisibleInGameLaunchMenu = false;
                        }

                        if (props.Length >= 4)
                        {
                            string visibleString = props[3];
                            if (visibleString == "V")
                                game.IsVisibleInGameList = true;
                            else
                                game.IsVisibleInGameList = false;
                        }
     
                    }
                }
            }

            if(emptyGamesCount == GbxGames.Count)
                Properties.Settings.Default.IsFirstRun = true;

        }
    }
}
