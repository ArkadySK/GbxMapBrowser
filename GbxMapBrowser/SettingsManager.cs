using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GbxMapBrowser
{
    internal static class SettingsManager
    {
        public enum SettingState
        {
            Unknown = 0,
            NotLoaded = 1,
            Loaded = 2,
            Saved = 3,
            Error = 4
        }

        static string settingsFilePath = Directory.GetCurrentDirectory() + "\\config\\settings.dat";
        static string settingsFolderPath = Directory.GetCurrentDirectory() + "\\config";

        public static SettingState SettingsState = SettingsManager.SettingState.NotLoaded;
        static public void LoadSettingsFromFile(GbxGameController controller)
        {
            if (!File.Exists(settingsFilePath))
            {
                Properties.Settings.Default.Reset();
                Properties.Settings.Default.IsFirstRun = true;
                SettingsState = SettingState.NotLoaded;
                return;
            }

            var settingsText = File.ReadAllLines(settingsFilePath);

            int emptyGamesCount = 0;
            foreach (var line in settingsText)
            {
                foreach (GbxGame game in controller.GbxGames)
                {
                    if (line.Contains(game.Name)) //game is found
                    {
                        string newline = line.Replace(game.Name + ": ", "");
                        string[] props = newline.Split('|');

                        if (props.Length == 0) // this part of code is used for old type of setting file
                        {
                            game.MapsFolder = line.Replace(game.Name + ": ", "");
                            continue;
                        }

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

                        if (props.Length >= 3)
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

            if (emptyGamesCount == controller.GbxGames.Count)
                Properties.Settings.Default.IsFirstRun = true;


        }

        public static void SaveSettings(GbxGameController controller)
        {
            string currentPath = Directory.GetCurrentDirectory();
            if (!Directory.Exists(settingsFolderPath))
                Directory.CreateDirectory(settingsFolderPath);

            List<string> settingsText = new List<string>();
            foreach (GbxGame game in controller.GbxGames)
            {
                if (game is CustomGbxGame)
                    continue;
                string enabledString = "N";
                if (game.IsVisibleInGameLaunchMenu)
                    enabledString = "E";
                string visibleInGameListString = "N";
                if (game.IsVisibleInGameList)
                    visibleInGameListString = "V";

                settingsText.Add(game.Name + ": " + game.MapsFolder + "|" + game.ExeLocation + "|" + enabledString + "|" + visibleInGameListString);
            }
            File.WriteAllLinesAsync(settingsFilePath, settingsText);
        }
    }
}
