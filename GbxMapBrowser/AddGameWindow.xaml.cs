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
        CustomGbxGame game;
        public AddGameWindow(GbxGameController gbxGameController, GbxGame baseGame)
        {
            InitializeComponent();
            GameController = gbxGameController;
            game = new CustomGbxGame(nameTextBox.Text, baseGame);
            game.Name = baseGame.Name + " New";
            DataContext = game;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            game.IsVisibleInGameList = false;
            game.GetInstallationDialog();
            GameController.GbxGames.Add(game);
            this.Close();
        }
    }
}
