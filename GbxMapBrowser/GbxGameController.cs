using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
            AddGbxGame("TM Nations Forever", Properties.Settings.Default.TMNationsForeverFolder, "TmForever.exe");
            AddGbxGame("TM United Forever", Properties.Settings.Default.TMUnitedForeverFolder, "TmForever.exe");
            AddGbxGame("ManiaPlanet", Properties.Settings.Default.ManiaPlanetFolder, "ManiaPlanet.exe");
            AddGbxGame("TM Turbo", Properties.Settings.Default.TMTurboFolder, "TrackmaniaTurbo.exe");
            AddGbxGame("TM 2020", Properties.Settings.Default.TMNextFolder, "Trackmania.exe");
        }
    }
}
