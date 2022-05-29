using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GbxMapBrowser
{
    public class GbxGameViewModel: INotifyPropertyChanged
    {
        internal void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<GbxGame> GbxGames { get; private set; } = new ObservableCollection<GbxGame>();
        public GbxGame SelectedGbxGame { get; set; } = new GbxGame();

        public event PropertyChangedEventHandler PropertyChanged;

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
