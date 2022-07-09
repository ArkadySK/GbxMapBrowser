using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
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
            Game.SetCustomIcon();
        }

        private void resetIconButton_Click(object sender, RoutedEventArgs e)
        {
            Game.Icon = new BitmapImage(new Uri(SettingsManager.DefaultGameIconPath));
        }
    }
}
