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

using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Extensions;
using GBX.NET.Engines.MwFoundations;
using System.IO;

namespace GbxMapBrowser
{
    /// <summary>
    /// Interaction logic for GbxInfoPage.xaml
    /// </summary>
    public partial class GbxInfoPage : Page
    {
        public GbxInfoPage(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            if (!File.Exists(filePath)) return;
            InitializeComponent();

            Node gbx;
            CGameCtnChallenge challenge = null;
            try
            {
                gbx = GameBox.Parse(filePath);
                if (gbx is CGameCtnChallenge cGameCtnChallenge)
                    challenge = cGameCtnChallenge;
            }
            catch
            {
                MessageBox.Show("An error happened while reading \"" + filePath + "\".", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            TreeViewItem curTreeViewItem = new TreeViewItem() { Header = "Challenge" };
            curTreeViewItem.IsExpanded = true;
            PopulateTreeView(null, curTreeViewItem, challenge);
        }

        int maximumDepth = 8;
        int curDepth = 0;

        void PopulateTreeView(TreeViewItem parentTreeViewItem, TreeViewItem curTreeViewItem, object classToExplore)
        {

            if (parentTreeViewItem == null)
                dataTreeView.Items.Add(curTreeViewItem);
            else
                parentTreeViewItem.Items.Add(curTreeViewItem);

            if (classToExplore == null)
                return;

            var properties = classToExplore.GetType().GetProperties();
            foreach (System.Reflection.PropertyInfo p in properties)
            {
                if (p == null) continue;
                if (!p.CanRead) continue;


                if (p.PropertyType == typeof(string))
                {
                    var stringValue = (string)p.GetValue(classToExplore);
                    if(string.IsNullOrWhiteSpace(stringValue))
                        PopulateTreeView(curTreeViewItem, new TreeViewItem() { Header = $"{p.Name}: (empty)", Opacity = 0.5 }, null);
                    else
                        PopulateTreeView(curTreeViewItem, new TreeViewItem() { Header = $"{p.Name}: { stringValue }", ToolTip=stringValue }, null);
                }
                else if (p.PropertyType.IsValueType)
                {
                    try
                    {
                        var name = $"{p.Name}: {p.GetValue(classToExplore)}";
                        PopulateTreeView(curTreeViewItem, new TreeViewItem() { Header = name }, null);
                    }
                    catch { }
                }
                else if (p.PropertyType.IsEnum)
                {
                    PopulateTreeView(curTreeViewItem, new TreeViewItem() { Header = p.Name, Foreground = Brushes.GreenYellow }, p.PropertyType.GetEnumValues());
                }
                else if (p.PropertyType.IsArray)
                {
                    PopulateTreeView(curTreeViewItem, new TreeViewItem() { Header = p.Name, Foreground = Brushes.GreenYellow }, (Array)p.GetValue(classToExplore));

                }
                else if (p.PropertyType.IsClass)
                {
                    if (curDepth >= maximumDepth) return;
                        curDepth++;
                    try
                    {
                        PopulateTreeView(curTreeViewItem, new TreeViewItem() { Header = p.Name, FontWeight = FontWeights.Bold }, p.GetValue(classToExplore));
                        curDepth--;
                    }
                        catch { }
                }
            }
        }


        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            //infoTextBlock.Text = "";
            GC.Collect();
        }
    }
}
