using System;
using System.Windows.Media;
using System.IO;
using GBX.NET;
using GBX.NET.Engines.Game;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using GBX.NET.Engines.MwFoundations;

namespace GbxMapBrowser
{
    public class MapInfo
    {
        public string MapName { get; }
        public string ExactMapName { get; }
        public DateTime DateModified { get; }
        public long FileSize { get; }
        public string FileSizeString
        {
            get
            {
                return FileOperations.SizeToString(FileSize);
            }
        }
        public string DateModifiedString { get {
                if (DateModified == null) return "NO DATE";
                return DateModified.ToString();
            } }
        public string MapFullName { get; }
        private string shortName;
        public string Author { get; }
        public string CopperPrice { get; }
        public string MapType { get; }
        public string Titlepack { get; }
        public Uri MoodIcon { get; }
        public string Description { get; }
        public string ObjectiveBronze { get; }
        public string ObjectiveSilver { get; }
        public string ObjectiveGold { get; }
        public string ObjectiveAuthor { get; }
        public Uri EnviImage { get; }
        public ImageSource MapThumbnail { get; }
        public bool IsWorking { get; }

        public MapInfo(string fullnamepath, bool basicInfoOnly)
        {
            CMwNod gbx;
            shortName = fullnamepath.Split("\\").Last();
            MapFullName = fullnamepath;
            FileInfo mapfileInfo = new FileInfo(fullnamepath);
            DateModified = mapfileInfo.LastWriteTime;
            FileSize = mapfileInfo.Length;

            try
            {
                if(basicInfoOnly)
                gbx = GameBox.ParseNodeHeader(fullnamepath);
                else
                gbx = GameBox.ParseNode(fullnamepath);
                IsWorking = true;
            }
            catch (Exception e)
            {
                MapName = "ERROR (" + shortName + ")";
                EnviImage = new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Error.png");
                Debug.WriteLine("Error: Map '" + fullnamepath + "' - impossible to load" + Environment.NewLine + e.Message);
                IsWorking = false;
                return;
            }

            if (gbx is CGameCtnChallenge gbxMap)
            {
                CGameCtnChallenge challenge = gbxMap;

                MapName = ToReadableText(challenge.MapName);
                ExactMapName = challenge.MapName;
                Titlepack = challenge.TitleID;

                Uri enviImagePath = EnviManager.GetEnvironmentImagePath(challenge.Collection, Titlepack);
                EnviImage = enviImagePath;

                ObjectiveGold = TimeSpanToString(challenge.TMObjective_GoldTime);
                if (basicInfoOnly) return;
                ObjectiveBronze = TimeSpanToString(challenge.TMObjective_BronzeTime);
                ObjectiveSilver = TimeSpanToString(challenge.TMObjective_SilverTime);
                ObjectiveAuthor = TimeSpanToString(challenge.TMObjective_AuthorTime);

                if (!string.IsNullOrEmpty(challenge.Comments))
                    Description = ToReadableText(challenge.Comments);
                MoodIcon = MoodManager.GetMoodImagePath(challenge.Decoration.Value.ToString());

                if (string.IsNullOrEmpty(challenge.AuthorNickname))
                    Author = challenge.AuthorLogin;
                else
                    Author = ToReadableText(challenge.AuthorNickname);

                CopperPrice = challenge.Cost.ToString();
                
                if (string.IsNullOrEmpty(challenge.ChallengeParameters.MapType))
                {
                    if (challenge.Mode.HasValue)
                        MapType = challenge.Mode.Value.ToString();
                }
                else
                    MapType = challenge.ChallengeParameters.MapType;

                if (challenge.Thumbnail == null) return;
                var thumbnailMemoryStream = new MemoryStream(challenge.Thumbnail);

                if (thumbnailMemoryStream == null) throw new Exception("buffer is empty");
                
                Bitmap mapThumbnail = new Bitmap(new StreamReader(thumbnailMemoryStream).BaseStream);
                MapThumbnail = ConvertToImageSource(mapThumbnail);
                MapThumbnail.Freeze();

            }
            else if (gbx is CGameCtnReplayRecord gbxReplay)
            {
                CGameCtnReplayRecord replay = gbxReplay;
                EnviImage = new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Replay.png");
                MapName = shortName.Replace(".Replay.Gbx", "", StringComparison.OrdinalIgnoreCase);
                ObjectiveGold = TimeSpanToString(replay.Time);
                if (basicInfoOnly) return;
                MapThumbnail = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Replay.png"));
                MapThumbnail.Freeze();
                Author = ToReadableText(replay.AuthorNickname);
                Titlepack = replay.TitleID;
            }
        }

        string TimeSpanToString(TimeSpan? timeSpan)
        {
            if (!timeSpan.HasValue) return "-:--.---";
            TimeSpan time = timeSpan.GetValueOrDefault();
            return time.ToString("hh':'mm':'ss");
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

        BitmapImage ConvertToImageSource(Bitmap src)
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


        public void OpenMap(GbxGame selGame)
        {
            string path = selGame.InstalationFolder + "\\" + selGame.TargetExeName;

            ProcessStartInfo gameGbxStartInfo = new ProcessStartInfo(path, "/useexedir /singleinst /file=\"" + MapFullName + "\"");
            Process gameGbx = new Process();
            gameGbx.StartInfo = gameGbxStartInfo;
            gameGbx.Start();
        }
    }
}
