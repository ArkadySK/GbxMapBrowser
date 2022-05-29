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
        GbxGameViewModel GameController;
        CustomGbxGame game;
        public AddGameWindow(GbxGameViewModel gbxGameController, GbxGame baseGame)
        {
            InitializeComponent();
            GameController = gbxGameController;
            game = new CustomGbxGame(nameTextBox.Text, baseGame);
            game.Name = baseGame.Name + " New";
            game.IsVisibleInGameList = false;
            game.IsVisibleInGameLaunchMenu = true;
            DataContext = game;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {       
            
            GameController.GbxGames.Add(game);
            this.Close();
        }

        private void changeMapFolder_Click(object sender, RoutedEventArgs e)
        {
            game.IsVisibleInGameList = false;
            game.SetCustomMapsFolder();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void changeExeButton_Click(object sender, RoutedEventArgs e)
        {
            game.GetInstallationAndMapFolderDialog();
        }
    }
}
