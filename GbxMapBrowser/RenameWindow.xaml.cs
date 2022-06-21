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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GbxMapBrowser
{
    /// <summary>
    /// Interaction logic for RenamePage.xaml:
    /// </summary>
    public partial class RenameWindow : Window
    {
        public string newName { get; set; }
        bool isFile = true;

        public RenameWindow(string oldName, bool IsFile)
        {
            if (isFile)
                this.Title = "Rename File";
            else
                this.Title = "Rename Map";

            InitializeComponent();
            isFile = IsFile;
            oldNameTextBox.Text = oldName;
            newNameTextBox.Text = oldName;
            newNameTextBox.Select(0, oldName.Length);
            newNameTextBox.Focus();
        }

        /// <summary>
        /// Rename item and close window if successfull
        /// </summary>
        private void SaveChanges()
        {
            string newNametemp = newNameTextBox.Text;
            if (!isFile)
            {
                newName = newNametemp;
                this.Close();
                return;
            }

            if (newNametemp.EndsWith("Map.Gbx") || newNametemp.EndsWith("Replay.Gbx"))
            {
                newName = newNametemp;
                this.Close();
            }
            else
                MessageBox.Show("Map file must end with '.Map.Gbx'!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveChanges();
        }

        private void newNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                SaveChanges();
            }
        }
    }
}
