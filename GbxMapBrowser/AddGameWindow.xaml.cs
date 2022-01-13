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
    /// Interaction logic for AddGameWindow.xaml
    /// </summary>
    public partial class AddGameWindow : Window
    {
        GbxGameController GameController;
        GbxGame baseGbxGame;
        public AddGameWindow(GbxGameController gbxGameController, GbxGame baseGame)
        {
            InitializeComponent();
            baseGbxGame = baseGame;
            nameTextBox.Text = baseGbxGame.Name + " New";
            GameController = gbxGameController;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CustomGbxGame game = new CustomGbxGame(nameTextBox.Text, baseGbxGame);
            game.IsVisibleInGameList = false;
            game.GetInstallationDialog();
            GameController.GbxGames.Add(game);
        }
    }
}
