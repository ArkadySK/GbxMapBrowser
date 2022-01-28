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

            SettingsManager.LoadAllSettingsFromFile(this);
        }
    }
}
