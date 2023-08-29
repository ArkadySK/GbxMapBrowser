using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace GbxMapBrowser
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly GbxGameViewModel _gbxGameViewModel;

        public SettingsWindow(GbxGameViewModel gbxGameViewModel)
        {
            InitializeComponent();
            if (Properties.Settings.Default.IsFirstRun)
            {
                Title = "Settings - configure your games first!";
                welcomeStackPanel.Visibility = Visibility.Visible;
            }
            _gbxGameViewModel = gbxGameViewModel;
            DataContext = _gbxGameViewModel;
            //SettingsManager.LoadAllSettingsFromFile(gbxGameController);
        }

        #region SaveAndClose
        bool CanCloseWindow()
        {
            if (Properties.Settings.Default.IsFirstRun)
            {
                MessageBoxResult result = MessageBox.Show("You have to find the location of atleast one game!" + Environment.NewLine + Environment.NewLine + "Close program?", "Can't continue to use the application.", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                    return true;
                else
                {
                    Application.Current.Shutdown();
                }
            }
            return false;
        }

        void SaveSettings()
        {
            Properties.Settings.Default.IsFirstRun = true;
            // Check if the locations exist to avoid errors
            foreach (GbxGame game in _gbxGameViewModel.GbxGames)
            {
                if (game is CustomGbxGame)
                {
                    if (!Directory.Exists(game.MapsFolder))
                        game.IsVisibleInGameList = false;
                    if (!File.Exists(game.ExeLocation))
                        game.IsVisibleInGameLaunchMenu = false;
                }
                else
                {
                    if (!Directory.Exists(game.MapsFolder) || !File.Exists(game.ExeLocation))
                    {
                        game.IsVisibleInGameLaunchMenu = false;
                        game.IsVisibleInGameList = false;
                    }
                }
            }

            // Check if there is atleast one game configured, but dont count custom games
            foreach (var gbxGame in _gbxGameViewModel.GbxGames)
            {
                if (gbxGame is CustomGbxGame) continue;
                if (gbxGame.IsVisibleInGameLaunchMenu || gbxGame.IsVisibleInGameList) Properties.Settings.Default.IsFirstRun = false;
            }


            Properties.Settings.Default.Save();
            SettingsManager.SaveAllSettings(_gbxGameViewModel);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
            e.Cancel = CanCloseWindow();
        }
        #endregion

        #region GameProperties
        private void AddGame()
        {
            var addWindow = new EditGameWindow();
            addWindow.Owner = this;
            bool? result = addWindow.ShowDialog();
            if (!result.HasValue) return;
            if (result.Value == false) return;

            var game = addWindow.Game;

            // Test if the name is not same as some other game by searching its name in the game list
            if (_gbxGameViewModel.FindGameByName(game.Name) is not null)
            {
                MessageBox.Show("The game with name '" + game.Name + "' already exists!\nPlease use different name.", "Cannot add game", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _gbxGameViewModel.GbxGames.Add(game);
        }

        private void EditGame(GbxGame game)
        {
            if (game == null)
            {
                MessageBox.Show("Please select a game from list.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (game is CustomGbxGame)
            {
                var customGame = game.Clone() as CustomGbxGame;

                EditGameWindow editGameWindow = new EditGameWindow(customGame);
                editGameWindow.Owner = this;
                bool? result = editGameWindow.ShowDialog();
                if (!result.HasValue)
                    return;
                if (result.Value == false)
                    return;
                // If the game is changed, replace the game in the game list
                int replacementIndex = _gbxGameViewModel.GbxGames.IndexOf(game);
                _gbxGameViewModel.GbxGames[replacementIndex] = customGame;
            }
            else if (game is GbxGame)
            {
                game.GetInstallationAndMapFolderDialog();
            }
        }

        private void RemoveGame(GbxGame game)
        {
            if (game == null)
            {
                MessageBox.Show("Please select a game from list.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (game is CustomGbxGame)
            {
                _gbxGameViewModel.GbxGames.Remove(game);
                game = null;
                return;
            }
            game.ExeLocation = null;
            game.IsVisibleInGameLaunchMenu = false;

            listView.ItemsSource = null;
            listView.ItemsSource = _gbxGameViewModel.GbxGames;
        }

        #endregion

        #region ButtonEvents
        private void buttonRemoveGame_Click(object sender, RoutedEventArgs e)
        {
            Button curButton = (Button)sender;
            Grid parentGrid = (Grid)curButton.Parent;
            string selectedName = (parentGrid.Children[0] as Label).Content.ToString();

            var selGame = _gbxGameViewModel.FindGameByName(selectedName);
            RemoveGame(selGame);
        }

        private void ButtonSaveAllChanges_Click(object sender, RoutedEventArgs e)
        {
            Close(); //not only close this window, but it will launch the closing event with save method
        }

        private void listView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selGame = listView.SelectedItem as GbxGame;

            EditGame(selGame);

            listView.ItemsSource = null;
            listView.ItemsSource = _gbxGameViewModel.GbxGames;
        }

        private void addCustomGameButton_Click(object sender, RoutedEventArgs e)
        {
            AddGame();
        }

        private void buttonConfigureGame_Click(object sender, RoutedEventArgs e)
        {
            Button curButton = (Button)sender;
            Grid parentGrid = (Grid)curButton.Parent;
            Label nameLabel = parentGrid.Children[0] as Label;
            if (nameLabel.Content is null) return;
            string selectedName = nameLabel.Content.ToString();
            var selGame = _gbxGameViewModel.FindGameByName(selectedName);

            EditGame(selGame);

            listView.ItemsSource = null;
            listView.ItemsSource = _gbxGameViewModel.GbxGames;
        }
        #endregion

        private void listView_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (listView.SelectedItem is null) return;

            var selGame = (GbxGame)listView.SelectedItem;

            if (e.Key == System.Windows.Input.Key.Delete)
            {
                RemoveGame(selGame);
            }
            else if (e.Key == System.Windows.Input.Key.Enter)
            {
                EditGame(selGame);
            }
        }
    }
}
