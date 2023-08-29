using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GbxMapBrowser
{
    /// <summary>
    /// Interaction logic for EditGameWindow.xaml
    /// </summary>
    public partial class EditGameWindow : Window
    {
        internal CustomGbxGame Game { get; private set; }
        /// <summary>
        /// Add new CustomGbxGame
        /// </summary>
        internal EditGameWindow()
        {
            InitializeComponent();
            Game = new CustomGbxGame();
            this.DataContext = Game;
            Title = "Add new game...";
            Game.IsVisibleInGameLaunchMenu = true;
            Game.IsVisibleInGameList = true;
            ResetIcon();
        }
        /// <summary>
        /// Edit existing CustomGbxGame
        /// </summary>
        /// <param name="game">A game to edit</param>
        internal EditGameWindow(CustomGbxGame game)
        {
            InitializeComponent();
            this.Game = game;
            this.DataContext = this.Game;
            Title = "Edit " + game.Name;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Game.Name))
            {
                MessageBox.Show("Please give your game a name", "Cannot save game", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(Game.MapsFolder) && string.IsNullOrWhiteSpace(Game.ExeLocation))
            {
                MessageBox.Show("Please choose an executable file or a maps folder for your game", "Cannot save game", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(Game.MapsFolder))
                Game.IsVisibleInGameLaunchMenu = false;
            if (string.IsNullOrWhiteSpace(Game.ExeLocation))
                Game.IsVisibleInGameLaunchMenu = false;

            this.DialogResult = true;
            this.Close();
        }

        private void changeMapFolder_Click(object sender, RoutedEventArgs e)
        {
            Game.IsVisibleInGameList = false;
            Game.SetCustomMapsFolder();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Game = null;
            this.DialogResult = false;
            Close();
        }

        private void changeExeButton_Click(object sender, RoutedEventArgs e)
        {
            Game.SetCustomExe();
        }

        private void chageIconButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Game.SetCustomIcon();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void resetIconButton_Click(object sender, RoutedEventArgs e)
        {
            ResetIcon();
        }

        private void ResetIcon()
        {
            Game.Icon = new BitmapImage(new Uri(SettingsManager.DefaultGameIconPath));
        }
    }
}
