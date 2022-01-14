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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.IsFirstRun = true;
            foreach (GbxGame game in GbxGameController.GbxGames)
            {
                if (String.IsNullOrEmpty(game.InstalationFolder)) game.IsEnabled = false;
                if (!Directory.Exists(game.InstalationFolder)) game.IsEnabled = false;
            }

            foreach (var gbxGame in GbxGameController.GbxGames)
            {
                if(gbxGame.IsEnabled) Properties.Settings.Default.IsFirstRun = false;
            }

            if (Properties.Settings.Default.IsFirstRun)
            {
                e.Cancel = true;
                MessageBoxResult result = MessageBox.Show("You have to find the location of atleast one game!" + Environment.NewLine + Environment.NewLine + "Close program?", "Can't continue to use the application.", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if(result == MessageBoxResult.Yes){
                    e.Cancel = false;
                    App.Current.Shutdown();
                }
            }
            Properties.Settings.Default.Save();
            GbxGameController.SaveSettings();
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
            selGame.IsEnabled = false;
            listView.ItemsSource = null;
            listView.ItemsSource = GbxGameController.GbxGames;

        }


        private void ButtonSaveAllChanges_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
