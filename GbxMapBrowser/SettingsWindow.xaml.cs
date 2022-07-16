﻿using System;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace GbxMapBrowser
{
    /// <summary>
    /// Interaction logic for GbxGamesWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        GbxGameViewModel GbxGameViewModel;

        public SettingsWindow(GbxGameViewModel gbxGameViewModel)
        {
            InitializeComponent();
            if (Properties.Settings.Default.IsFirstRun)
            {
                Title = "Settings - configure your games first!";
                welcomeStackPanel.Visibility = Visibility.Visible;
            }
            GbxGameViewModel = gbxGameViewModel;
            DataContext = GbxGameViewModel;
            //SettingsManager.LoadAllSettingsFromFile(gbxGameController);
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
            foreach (GbxGame game in GbxGameViewModel.GbxGames)
            {
                if (!Directory.Exists(game.InstalationFolder) || !File.Exists(game.ExeLocation))
                {
                    game.IsVisibleInGameLaunchMenu = false;
                    game.IsVisibleInGameList = false;
                }
            }

            foreach (var gbxGame in GbxGameViewModel.GbxGames)
            {
                if (gbxGame.IsVisibleInGameLaunchMenu || gbxGame.IsVisibleInGameList) Properties.Settings.Default.IsFirstRun = false;
            }

            
            Properties.Settings.Default.Save();
            SettingsManager.SaveAllSettings(GbxGameViewModel);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
            e.Cancel = CanCloseWindow();
        }

        private void EditGame(GbxGame selGame)
        {
            if (selGame == null)
            {
                MessageBox.Show("Please select a game from list.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selGame is CustomGbxGame)
            {
                var customGame = selGame.Clone() as CustomGbxGame;

                EditGameWindow editGameWindow = new EditGameWindow(customGame);
                editGameWindow.Owner = this;
                bool? result = editGameWindow.ShowDialog();
                if (!result.HasValue)
                    return;
                if (result.Value == false)
                    return;
                // If the game is changed, replace the game in the game list
                int replacementIndex = GbxGameViewModel.GbxGames.IndexOf(selGame);
                GbxGameViewModel.GbxGames[replacementIndex] = customGame;
            }
            else if(selGame is GbxGame)
            {
                selGame.GetInstallationAndMapFolderDialog();
            }
        }

        private void buttonRemoveGame_Click(object sender, RoutedEventArgs e)
        {
            Button curButton = (Button)sender;
            Grid parentGrid = (Grid)curButton.Parent;
            string selectedName = (parentGrid.Children[0] as Label).Content.ToString();

            var selGame = GbxGameViewModel.FindSelectedGameByName(selectedName);
            if (selGame == null)
            {
                MessageBox.Show("Impossible to remove - please select a game from list.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selGame is CustomGbxGame)
            {
                GbxGameViewModel.GbxGames.Remove(selGame);
                selGame = null;
                return;
            }
            selGame.ExeLocation = null;
            selGame.IsVisibleInGameLaunchMenu = false;

            listView.ItemsSource = null;
            listView.ItemsSource = GbxGameViewModel.GbxGames;

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
            listView.ItemsSource = GbxGameViewModel.GbxGames;
        }

        private void addCustomGameButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new EditGameWindow();
            addWindow.Owner = this;
            bool? result = addWindow.ShowDialog();
            if (!result.HasValue) return;
            if (result.Value == true)
                GbxGameViewModel.GbxGames.Add(addWindow.Game);
        }

        private void buttonConfigureGame_Click(object sender, RoutedEventArgs e)
        {
            Button curButton = (Button)sender;
            Grid parentGrid = (Grid)curButton.Parent;
            Label nameLabel = parentGrid.Children[0] as Label;
            if (nameLabel.Content is null) return;
            string selectedName = nameLabel.Content.ToString();
            var selGame = GbxGameViewModel.FindSelectedGameByName(selectedName);

            EditGame(selGame);

            listView.ItemsSource = null;
            listView.ItemsSource = GbxGameViewModel.GbxGames;
        }
    }
}