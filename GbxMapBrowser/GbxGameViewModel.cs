using System.Collections.ObjectModel;
using System.Linq;

namespace GbxMapBrowser
{
    public class GbxGameViewModel
    {
        public ObservableCollection<GbxGame> GbxGames { get; private set; } = [];
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

        public GbxGame FindGameByName(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? null : GbxGames.FirstOrDefault(x => x.Name == name);
        }
    }
}
