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

namespace GbxMapBrowser
{
    class MapInfo
    {
        public string MapName { get; }
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
            GameBox gbx = new GameBox();
            shortName = fullnamepath.Split("\\").Last();

            try
            {
                gbx = GameBox.Parse(fullnamepath);
            }
            catch (Exception e)
            {
                MapName = "ERROR (" + shortName + ")";
                EnviImage = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Error.png"));
                Debug.WriteLine("Error: Map '" + fullnamepath + "' - impossible to load" + Environment.NewLine + e.Message);
                return;
            }

            MapFullName = fullnamepath;

            if (gbx is GameBox<CGameCtnChallenge> gbxMap)
            {
                CGameCtnChallenge challenge = gbxMap.MainNode;
                
                MapName = ToReadableText(challenge.MapName);
                if (string.IsNullOrEmpty(challenge.AuthorNickname))
                    Author = challenge.AuthorLogin;
                else
                    Author = ToReadableText(challenge.AuthorNickname);

                CopperPrice = challenge.Cost.ToString();

                if (string.IsNullOrEmpty(challenge.ChallengeParameters.MapType))
                {
                    if (challenge.Mode.HasValue)
                        MapType = "Gamemode: " + challenge.Mode.Value.ToString();
                }
                else
                    MapType = "Gamemode: " + challenge.ChallengeParameters.MapType;

                ObjectiveBronze = challenge.TMObjective_BronzeTime.GetValueOrDefault().ToStringTM();
                ObjectiveSilver = challenge.TMObjective_SilverTime.GetValueOrDefault().ToStringTM();
                ObjectiveGold = challenge.TMObjective_GoldTime.GetValueOrDefault().ToStringTM();
                ObjectiveAuthor = challenge.TMObjective_AuthorTime.GetValueOrDefault().ToStringTM();
                Titlepack = challenge.TitleID;
                EnviImage = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Data\\Environments\\" + challenge.Collection + ".dds"));

                var thumb_stream = new StreamReader(new MemoryStream(challenge.Thumbnail)).ReadToEnd();
                {
                    Bitmap mapThumbnail = new Bitmap(thumb_stream);
                    MapThumbnail = ConvertToImageSource(mapThumbnail);
                }

            }
            else if (gbx is GameBox<CGameCtnReplayRecord> gbxReplay)
            {
                CGameCtnReplayRecord replay = gbxReplay.MainNode;
                EnviImage = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Replay.png"));
                MapThumbnail = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Replay.png"));
                Author = ToReadableText(replay.AuthorNickname);
                MapName = shortName.Replace(".Replay.Gbx", "", StringComparison.OrdinalIgnoreCase);
                Titlepack = replay.TitleID;
            }
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
