using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Text;
using System.IO;
using GBX.NET;
using GBX.NET.Engines.Game;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using GBX.NET.Engines.MwFoundations;

namespace GbxMapBrowser
{
    class MapInfo
    {
        public string MapName { get; }
        public string ExactMapName { get; }
        public string MapFullName { get; }
        private string shortName;
        public string Author { get; }
        public string CopperPrice { get; }
        public string MapType { get; }
        public string Titlepack { get; }
        public string ObjectiveBronze { get; }
        public string ObjectiveSilver { get; }
        public string ObjectiveGold { get; }
        public string ObjectiveAuthor { get; }
        public ImageSource EnviImage { get; }
        public ImageSource MapThumbnail { get; }


        public MapInfo(string fullnamepath)
        {
            CMwNod gbx;
            shortName = fullnamepath.Split("\\").Last();
            
            try
            {
                gbx = GameBox.ParseNodeHeader(fullnamepath);
            }
            catch (Exception e)
            {
                MapName = "ERROR (" + shortName + ")";
                EnviImage = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Error.png"));
                Debug.WriteLine("Error: Map '" + fullnamepath + "' - impossible to load" + Environment.NewLine + e.Message);
                return;
            }

            MapFullName = fullnamepath;

            if (gbx is CGameCtnChallenge gbxMap)
            {
                CGameCtnChallenge challenge = gbxMap;
                
                MapName = ToReadableText(challenge.MapName);
                ExactMapName = challenge.MapName;
                if (string.IsNullOrEmpty(challenge.AuthorNickname))
                    Author = challenge.AuthorLogin;
                else
                    Author = ToReadableText(challenge.AuthorNickname);

                CopperPrice = challenge.Cost.ToString();
                /*
                if (string.IsNullOrEmpty(challenge.ChallengeParameters.MapType))
                {
                    if (challenge.Mode.HasValue)
                        MapType = "Gamemode: " + challenge.Mode.Value.ToString();
                }
                else
                    MapType = "Gamemode: " + challenge.ChallengeParameters.MapType;
                */
                ObjectiveBronze = TimeSpanToString(challenge.TMObjective_BronzeTime);
                ObjectiveSilver = TimeSpanToString(challenge.TMObjective_SilverTime);
                ObjectiveGold = TimeSpanToString(challenge.TMObjective_GoldTime);
                ObjectiveAuthor = TimeSpanToString(challenge.TMObjective_AuthorTime);
                Titlepack = challenge.TitleID;

                Uri enviImagePath = new Uri(Environment.CurrentDirectory + "\\Data\\Environments\\" + challenge.Collection + ".png");
                EnviImage = new BitmapImage(enviImagePath);

                if (challenge.Thumbnail == null) return;
                var thumbnailMemoryStream = new MemoryStream(challenge.Thumbnail);

                if (thumbnailMemoryStream == null) throw new Exception("HELO your buffer is empty :(");
                {
                    Bitmap mapThumbnail = new Bitmap(new StreamReader(thumbnailMemoryStream).BaseStream);
                    MapThumbnail = ConvertToImageSource(mapThumbnail);
                }

            }/*
            else if (gbx is CGameCtnReplayRecord gbxReplay)
            {
                CGameCtnReplayRecord replay = gbxReplay;
                EnviImage = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Replay.png"));
                MapThumbnail = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Replay.png"));
                Author = ToReadableText(replay.AuthorNickname);
                MapName = shortName.Replace(".Replay.Gbx", "", StringComparison.OrdinalIgnoreCase);
                Titlepack = replay.TitleID;
            }*/
        }

        string TimeSpanToString(TimeSpan? timeSpan)
        {
            if (!timeSpan.HasValue) return "-:--.---";
            return timeSpan.GetValueOrDefault().ToStringTM();
        }

        public void RenameAndSave(string newName)
        {
            GameBox gbx;

            try
            {
                gbx = GameBox.Parse(MapFullName);
            }
            catch (Exception e)
            {
                throw e;
            }

            if (gbx is GameBox<CGameCtnChallenge> gbxMap)
            {
                CGameCtnChallenge challenge = gbxMap.Node;
                challenge.MapName = newName;
                gbxMap.Save(MapFullName);
            }
            else
                throw new NotImplementedException("Only Maps could be renamed.");
            
        }

            public BitmapImage ConvertToImageSource(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        string ToReadableText(string defaultname)
        {
            string formattedName = defaultname;      
            formattedName = Formatter.Deformat(formattedName);
            return formattedName;
        }
    }
}
