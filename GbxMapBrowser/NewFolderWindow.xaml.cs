using System.Windows;
using System.Windows.Input;

namespace GbxMapBrowser
{
    /// <summary>
    /// Interaction logic for NewFolderWindow.xaml
    /// </summary>
    public partial class NewFolderWindow : Window
    {
        public string NewName { get; set; }
        public NewFolderWindow()
        {
            InitializeComponent();
        }

        private void SaveChanges()
        {
            if (nameTextBox.Text.Length == 0)
            {
                MessageBox.Show("Please enter a name", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NewName = nameTextBox.Text;
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveChanges();
        }

        private void NameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SaveChanges();
            }
        }
    }
}
