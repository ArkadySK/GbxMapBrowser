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
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.IsFirstRun = true;
            foreach (GbxGame game in GbxGameController.GbxGames)
            {
                if (String.IsNullOrEmpty(game.InstalationFolder)) game.IsEnabled = false;
                if (!Directory.Exists(game.InstalationFolder)) game.IsEnabled = false;
            }
            Properties.Settings.Default.TMForeverFolder = GbxGameController.GbxGames[0].InstalationFolder;
            Properties.Settings.Default.ManiaPlanetFolder = GbxGameController.GbxGames[1].InstalationFolder;
            Properties.Settings.Default.TMNextFolder = GbxGameController.GbxGames[2].InstalationFolder;

            foreach (var gbxGame in GbxGameController.GbxGames)
            {
                if(gbxGame.IsEnabled) Properties.Settings.Default.IsFirstRun = false;
            }

            if (Properties.Settings.Default.IsFirstRun)
            {
                e.Cancel = true;
                MessageBoxResult result = MessageBox.Show("You have to find the location of atleast one game!" + Environment.NewLine + Environment.NewLine + "Retry?", "Can't continue to use the application.", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                if(result == MessageBoxResult.Cancel){
                    e.Cancel = false;
                    App.Current.Shutdown();
                }
            }
            Properties.Settings.Default.Save();
        }

        private void ButtonChangeInstallLocation_Click(object sender, RoutedEventArgs e)
        {
            var selGame = (GbxGame)listView.SelectedItem;
            if(selGame==null)
            {
                MessageBox.Show("Can't change game folder - please select a game from list.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Title = "Please locate the " + selGame.TargetExeName + " (game: " + selGame.Name + ")",
            };
            //fixni to pls
            var dialogResult = openFileDialog.ShowDialog();
            if (!dialogResult.HasValue) return;

            if (dialogResult.Value) { 
                var exeName = openFileDialog.FileName.Split("\\").Last();
                if (exeName == selGame.TargetExeName)
                {
                    selGame.InstalationFolder = openFileDialog.FileName.Replace("\\"+exeName, "");
                    selGame.UpdateMapsFolder();
                    selGame.IsEnabled = true;
                }
                else
                    MessageBox.Show("Wrong exe file! '" + exeName + "' Executable names do not match.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }

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
            selGame.InstalationFolder = "";
            selGame.IsEnabled = false;
            listView.ItemsSource = null;
            listView.ItemsSource = GbxGameController.GbxGames;

        }

        private void ButtonSaveAllChanges_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
