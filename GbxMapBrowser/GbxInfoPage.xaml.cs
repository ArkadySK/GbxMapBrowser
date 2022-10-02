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
using System.Threading.Tasks;

namespace GbxMapBrowser
{
    /// <summary>
    /// Interaction logic for GbxInfoPage.xaml
    /// </summary>
    public partial class GbxInfoPage : Page
    {
        private CGameCtnChallenge challenge = null;
        string path;


        public GbxInfoPage(string filePath)
        {
            InitializeComponent();
            path = filePath;
        }


        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (!File.Exists(path)) return;


            Node gbx;
            try
            {
                gbx = GameBox.Parse(path);
                if (gbx is CGameCtnChallenge cGameCtnChallenge)
                    challenge = cGameCtnChallenge;
                else
                {
                    throw new Exception("Selected file is not challenge / map!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error happened while reading \"" + path + "\".\nError message: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TreeViewItem curTreeViewItem = new TreeViewItem() { Header = "Challenge" };
            curTreeViewItem.IsExpanded = true;
            PopulateTreeView(null, curTreeViewItem, challenge);
            await Task.CompletedTask;
        }

        int maximumDepth = 5;
        int curDepth = 0;

        private void PopulateTreeView(TreeViewItem parentTreeViewItem, TreeViewItem curTreeViewItem, object objectToExplore)
        {
            // Add treeview item
            if (parentTreeViewItem == null)
                dataTreeView.Items.Add(curTreeViewItem);
            else
                parentTreeViewItem.Items.Add(curTreeViewItem);

            if (objectToExplore == null)
                return;

            Type objectType = objectToExplore.GetType();

            // If the object is an array
            if (objectType.IsArray)
            {
                var array = (Array)objectToExplore;
                if (curDepth >= maximumDepth) return;
                if (array.Length > 400) return;
                curDepth++;
                foreach (var item in array)
                {
                    if (item is null) continue;
                    curTreeViewItem.Items.Add(new TreeViewItem() { Header = item.ToString() });
                }
                curDepth--;
                return;
            }
            // If the object is IEnumerable
            else if (objectType.IsGenericType)
            {
                if (objectType == typeof(System.Collections.IEnumerable))
                {
                    System.Collections.IEnumerable items = (System.Collections.IEnumerable)objectToExplore;
                    if (items is null)
                    {
                        PopulateTreeView(curTreeViewItem, new TreeViewItem() { Header = items.ToString() + " (empty)", Opacity = 0.5 }, null);
                        return;
                    }
                    int counter = 0;
                    int counterMax = 400;

                    if (curDepth < maximumDepth)
                    {
                        curDepth++;
                        foreach (var item in items)
                        {
                            if (counter < counterMax)
                            {
                                PopulateTreeView(curTreeViewItem, new TreeViewItem() { Header = item.ToString() }, item);
                                counter++;
                            }
                            else
                            {
                                PopulateTreeView(curTreeViewItem, new TreeViewItem() { Header = "nb of items has been reduced to improve performance", Opacity = 0.5 }, null);
                                break;
                            }
                        }
                        curDepth--;
                    }
                    return;
                }
            }
            // Make a list of chunks
            else if (objectType.BaseType == typeof(SortedSet<Chunk>))
            {
                SortedSet<Chunk> chunkSet = (SortedSet<Chunk>)objectToExplore;
                foreach (var chunk in chunkSet)
                    PopulateTreeView(curTreeViewItem, new TreeViewItem() { Header = chunk.ToString() }, chunk);
            }

            // If it has properties
            var properties = objectType.GetProperties();
            foreach (System.Reflection.PropertyInfo p in properties)
            {
                if (p == null) continue;
                if (!p.CanRead) continue;

                if (p.PropertyType == typeof(string))
                {
                    var stringValue = (string)p.GetValue(objectToExplore);
                    if (string.IsNullOrWhiteSpace(stringValue))
                        PopulateTreeView(curTreeViewItem, new TreeViewItem() { Header = $"{p.Name}: (empty)", Opacity = 0.5 }, null);
                    else
                    {
                        var shortStringValue = ShortenString(stringValue, 40);
                        PopulateTreeView(curTreeViewItem, new TreeViewItem() { Header = $"{p.Name}: { shortStringValue }", ToolTip = stringValue }, null);
                    }
                }
                else if (p.PropertyType.IsValueType)
                {
                    try
                    {
                        var name = $"{p.Name}: {p.GetValue(objectToExplore)}";
                        PopulateTreeView(curTreeViewItem, new TreeViewItem() { Header = name }, null);
                    }
                    catch { }
                }
                else if (p.PropertyType.IsEnum)
                {
                    PopulateTreeView(curTreeViewItem, new TreeViewItem() { Header = p.Name }, p.PropertyType.GetEnumValues());
                }
                else if (p.PropertyType.IsArray)
                {
                    // TO DO: FIX ARRAYS!
                    PopulateTreeView(curTreeViewItem, new TreeViewItem() { Header = p.Name }, (Array)p.GetValue(objectToExplore));
                }
                else /*if (p.PropertyType.IsClass)*/
                {
                    if (curDepth >= maximumDepth) return;
                    curDepth++;
                    try
                    {
                        PopulateTreeView(curTreeViewItem, new TreeViewItem() { Header = p.Name }, p.GetValue(objectToExplore));
                        curDepth--;
                    }
                    catch { }

                }
            }
        }

        string ShortenString(string text, int size)
        {
            if (text.Length <= size)
            {
                return text;
            }
            else
            {
                text = text.Substring(0, size - 3);
                return text + "...";
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            GC.Collect();
        }

    }
}
