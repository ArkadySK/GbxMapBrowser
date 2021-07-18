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
    public partial class RenamePage : Page
    {
        public RenamePage()
        {
            InitializeComponent();
            
        }
        public RenamePage(string oldName)
        {
            InitializeComponent();
            oldNameTextBox.Text = oldName;
            newNameTextBox.Text = oldName;
            newNameTextBox.Select(0, oldName.Length);
            newNameTextBox.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string newName = newNameTextBox.Text;
            if (newName.EndsWith("Map.Gbx") || newName.EndsWith("Replay.Gbx"))
                throw new NotImplementedException("the returning of value is not implemented yet");//TO DO: RETURN THE VALUE to parent thread
            else
                MessageBox.Show("Map file must end with '.Map.Gbx'!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
