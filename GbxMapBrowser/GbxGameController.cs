using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GbxMapBrowser
{
    public class GbxGameController
    {
        public ObservableCollection<GbxGame> GbxGames { get; } = new ObservableCollection<GbxGame>();
        public GbxGame SelectedGbxGame { get; set; } = new GbxGame();

        public void AddGbxGame(string name, string instalationfolder, string targetexename)
        {
            GbxGames.Add(new GbxGame(name, instalationfolder, targetexename));
        }
    }
}
