using System;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
using System.Linq;
using System.Collections.Generic;

namespace GbxMapBrowser
{
    /// <summary>
    /// Interaction logic for GbxGamesWindow.xaml
    /// </summary>
    public partial class GbxGamesWindow : Window
    {
        GbxGameController GbxGameController;

        public GbxGamesWindow(GbxGameController gbxGameController)
        {
            if (Properties.Settings.Default.IsFirstRun) Title = "Configure your games";
            InitializeComponent();
            GbxGameController = gbxGameController;
            DataContext = GbxGameController;
            GbxGameController.UpdateSettingsFromFile();
        }

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
            foreach (GbxGame game in GbxGameController.GbxGames)
            {
                if (!Directory.Exists(game.InstalationFolder) || !File.Exists(game.ExeLocation))
                {
                    game.IsVisibleInGameLaunchMenu = false;
                    game.IsVisibleInGameList = false;
                }
            }

            foreach (var gbxGame in GbxGameController.GbxGames)
            {
                if (gbxGame.IsVisibleInGameLaunchMenu || gbxGame.IsVisibleInGameList) Properties.Settings.Default.IsFirstRun = false;
            }

            
            Properties.Settings.Default.Save();
            GbxGameController.SaveSettings();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
            e.Cancel = CanCloseWindow();
        }

        private void ButtonChangeInstallLocation_Click(object sender, RoutedEventArgs e)
        {
            var selGame = (GbxGame)listView.SelectedItem;
            if(selGame==null)
            {
                MessageBox.Show("Can't change game folder - please select a game from list.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            selGame.GetInstallationDialog();
            listView.ItemsSource = null;
            listView.ItemsSource = GbxGameController.GbxGames;

        }

        private void ButtonResetInstallLocation_Click(object sender, RoutedEventArgs e)
        {
            var selGame = (GbxGame)listView.SelectedItem;
            if (selGame == null)
            {
                MessageBox.Show("Can't change game folder - please select a game from list.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            selGame.ExeLocation = null;
            selGame.IsVisibleInGameLaunchMenu = false;
            listView.ItemsSource = null;
            listView.ItemsSource = GbxGameController.GbxGames;

        }


        private void ButtonSaveAllChanges_Click(object sender, RoutedEventArgs e)
        {
            Close(); //not only close this window, but it will launch the closing event with save method
        }

        private void listView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selGame = (GbxGame)listView.SelectedItem;
            if (selGame == null) return;
            selGame.GetInstallationDialog();
            listView.ItemsSource = null;
            listView.ItemsSource = GbxGameController.GbxGames;
        }

        private void addCustomGameButton_Click(object sender, RoutedEventArgs e)
        {
            var selGame = (GbxGame)(listView.SelectedItem);
            if(selGame == null) return;
            var addWindow = new AddGameWindow(GbxGameController, selGame);
            addWindow.ShowDialog();
        }
    }
}
