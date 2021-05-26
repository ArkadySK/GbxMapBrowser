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
            foreach(GbxGame game in GbxGameController.GbxGames)
            {
                if (!Directory.Exists(game.InstalationFolder)) game.IsEnabled = false;
            }

            Properties.Settings.Default.TMForeverFolder = GbxGameController.GbxGames[0].InstalationFolder;
            Properties.Settings.Default.ManiaPlanetFolder = GbxGameController.GbxGames[1].InstalationFolder;
            Properties.Settings.Default.TMNextFolder = GbxGameController.GbxGames[2].InstalationFolder;
            Properties.Settings.Default.IsFirstRun = false;
            Properties.Settings.Default.Save();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
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
            if (openFileDialog.ShowDialog().Value) { 
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
    }
}
