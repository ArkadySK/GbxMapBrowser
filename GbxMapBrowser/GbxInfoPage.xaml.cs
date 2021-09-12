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

            try
            {
                CMwNod gbx = GameBox.Parse(filePath);

                foreach (var p in gbx.GetType().GetProperties())
                {
                    if (p == null) continue;
                    if (!p.CanRead) continue;

                    //var otherProperties = p.GetValue(gbx).GetType().GetProperties();
                    infoTextBlock.Text += string.Format("{0}: ({1})", p.Name, p.GetValue(gbx)) + Environment.NewLine + Environment.NewLine;
                    /*if(otherProperties.Length > 1)
                    {
                        infoTextBlock.Text += Environment.NewLine + p.Name;

                        foreach (var op in otherProperties)
                        {
                            if (op == null) continue;
                            if (!op.CanRead) continue;
                            infoTextBlock.Text += string.Format("   {0}: ({1})", op.Name, op.GetValue(gbx)) + Environment.NewLine + Environment.NewLine;

                        }
                    }*/
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
        }
    }
}
