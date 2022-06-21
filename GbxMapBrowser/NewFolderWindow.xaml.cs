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
    /// Interaction logic for NewFolderWindow.xaml
    /// </summary>
    public partial class NewFolderWindow : Window
    {
        public string newName { get; set; }
        public NewFolderWindow()
        {
            InitializeComponent();
        }

        private void SaveChanges()
        {
            if(nameTextBox.Text.Length == 0)
            {
                MessageBox.Show("Please enter a name", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            newName = nameTextBox.Text;
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveChanges();
        }

        private void nameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SaveChanges();
            }
        }
    }
}
