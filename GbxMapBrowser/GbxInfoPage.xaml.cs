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
                //infoTextBlock.Text += Environment.NewLine + "An error happened while reading \"" + filePath + "\".";
            }

            
            TreeViewItem curTreeViewItem = new TreeViewItem() { Header = "Challenge" };
            PopulateTreeView(null, curTreeViewItem, challenge);
        }

        void PopulateTreeView(TreeViewItem parentTreeViewItem, TreeViewItem curTreeViewItem, object classToExplore)
        {
            if (parentTreeViewItem == null)
                dataTreeView.Items.Add(curTreeViewItem);
            else
                parentTreeViewItem.Items.Add(curTreeViewItem);

            if (classToExplore == null)
                return;

            
            foreach (var p in classToExplore.GetType().GetProperties())
            {
                if (p == null) continue;
                if (!p.CanRead) continue;

                try
                {
                    if (p.PropertyType.IsGenericType)
                    {
                        var name = string.Format("{0}: ({1})", p.Name, p.GetValue(classToExplore));
                        PopulateTreeView(curTreeViewItem, new TreeViewItem() { Header = name }, null);

                    }
                    else if (p.PropertyType.IsEnum)
                    {
                        
                    }
                    else if (!p.PropertyType.IsClass)
                    {
                        PopulateTreeView(curTreeViewItem, new TreeViewItem() { Header = p.Name }, p);
                    }
                }
                catch
                {
                    curTreeViewItem.Items.Add("ERROR: " + p.Name + ": ???");
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
