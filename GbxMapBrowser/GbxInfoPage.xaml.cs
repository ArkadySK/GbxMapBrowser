using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.MwFoundations;
using GBX.NET.Serialization.Chunking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GbxMapBrowser
{
    /// <summary>
    /// Interaction logic for GbxInfoPage.xaml
    /// </summary>
    public partial class GbxInfoPage : Page
    {
        private CGameCtnChallenge _challenge = null;
        private readonly string _path;

        private int _curDepth = 0;
        private readonly int _maximumDepth = 5;

        public GbxInfoPage(string filePath)
        {
            InitializeComponent();
            _path = filePath;
        }


        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_path)) return;
            if (!File.Exists(_path)) return;


            CMwNod gbx;
            try
            {
                gbx = Gbx.ParseNode(_path);
                if (gbx is CGameCtnChallenge cGameCtnChallenge)
                    _challenge = cGameCtnChallenge;
                else
                {
                    throw new Exception("Selected file is not challenge / map!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error happened while reading \"" + _path + "\".\nError message: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            titleLabel.Content = "Loading...";
            await Task.Delay(1000);

            TreeViewItem curTreeViewItem = new TreeViewItem() { Header = "Challenge" };
            curTreeViewItem.IsExpanded = true;
            PopulateTreeView(null, curTreeViewItem, _challenge);
            await Task.CompletedTask;
            titleLabel.Content = "GBX Preview:";

        }


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
                if (_curDepth >= _maximumDepth) return;
                if (array.Length > 400) return;
                _curDepth++;
                foreach (var item in array)
                {
                    if (item is null) continue;
                    curTreeViewItem.Items.Add(new TreeViewItem() { Header = item.ToString() });
                }
                _curDepth--;
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

                    if (_curDepth < _maximumDepth)
                    {
                        _curDepth++;
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
                        _curDepth--;
                    }
                    return;
                }
            }
            // Make a list of chunks
            else if (objectType.BaseType == typeof(SortedSet<IChunk>))
            {
                SortedSet<IChunk> chunkSet = (SortedSet<IChunk>)objectToExplore;
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
                        PopulateTreeView(curTreeViewItem, new TreeViewItem() { Header = $"{p.Name}: {shortStringValue}", ToolTip = stringValue }, null);
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
                    if (_curDepth >= _maximumDepth) return;
                    _curDepth++;
                    try
                    {
                        PopulateTreeView(curTreeViewItem, new TreeViewItem() { Header = p.Name }, p.GetValue(objectToExplore));
                        _curDepth--;
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
