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

            try
            {
                gbx = GameBox.Parse(filePath);
                if (gbx is not CGameCtnChallenge challenge)
                    return;

                    //infoTextBlock.Text += "Chunks count: " + gbx.Chunks.Count;

                    foreach (var ch in gbx.Chunks)
                {
                    
                    
                    infoTextBlock.Text += Environment.NewLine + ch.ToString();
                    int i = 0;

                    {
                        foreach (var p in challenge.GetType().GetProperties())
                        {
                            if (p == null) continue;
                            if (!p.CanRead) continue;

                            try
                            {
                                infoTextBlock.Text += string.Format("{0}: ({1})", p.Name, p.GetValue(challenge)) + Environment.NewLine + Environment.NewLine;
                            }
                            catch
                            {
                                infoTextBlock.Text += "ERROR: " + p.Name + ": ???";
                            }

                            finally {                              
                                i++; 
                            }
                        }
                    }
                }


                
                /*
                if (gbx is CGameCtnChallenge challenge)
                {
                    DataContext = challenge;
                    challenge.Read(new GameBoxReader(new FileStream(filePath, FileMode.Open)));
                    challenge.Chunks.DiscoverAll(false);
                }*/
            }
            catch
            {
                infoTextBlock.Text += Environment.NewLine + "An error happened while reading \"" + filePath + "\".";
            }
            finally
            {
                gbx = null;
                GC.Collect();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            infoTextBlock.Text = "";
            GC.Collect();
        }
    }
}
