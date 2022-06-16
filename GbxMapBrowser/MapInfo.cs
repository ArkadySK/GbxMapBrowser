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
using TmEssentials;
using System.Threading.Tasks;

namespace GbxMapBrowser
{
    public class MapInfo: FolderAndFileInfo
    {
        public string ExactMapName { get; } 
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
            Node gbx;
            shortName = fullnamepath.Split("\\").Last();
            FullPath = fullnamepath;
            FileInfo mapfileInfo = new FileInfo(fullnamepath);
            DateModified = mapfileInfo.LastWriteTime;
            DateCreated = mapfileInfo.CreationTime;
            Size = mapfileInfo.Length;

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
                Name = "ERROR (" + shortName + ")";
                EnviImage = new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Error.png");
                Debug.WriteLine("Error: Map '" + fullnamepath + "' - impossible to load" + Environment.NewLine + e.Message);
                IsWorking = false;
                return;
            }

            if (gbx is CGameCtnChallenge gbxMap)
            {
                CGameCtnChallenge challenge = gbxMap;

                Name = ToReadableText(challenge.MapName);
                ExactMapName = challenge.MapName;
                Titlepack = challenge.TitleID;

                Uri enviImagePath = EnviManager.GetEnvironmentImagePath(challenge.Collection, Titlepack);
                if (File.Exists(enviImagePath.AbsolutePath))
                    EnviImage = enviImagePath;
                else
                    EnviImage = new Uri(Environment.CurrentDirectory + "\\Data\\Environments\\Unknown.png");

                ObjectiveGold = TimeSpanToString(challenge.TMObjective_GoldTime);
                if (basicInfoOnly) return;
                ObjectiveBronze = TimeSpanToString(challenge.TMObjective_BronzeTime);
                ObjectiveSilver = TimeSpanToString(challenge.TMObjective_SilverTime);
                ObjectiveAuthor = TimeSpanToString(challenge.TMObjective_AuthorTime);

                if (!string.IsNullOrEmpty(challenge.Comments))
                    Description = ToReadableText(challenge.Comments);
                MoodIcon = MoodManager.GetMoodImagePath(challenge.Decoration.ToString());

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
                Name = shortName.Replace(".Replay.Gbx", "", StringComparison.OrdinalIgnoreCase);
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
            return time.ToTmString();
        }

        public void RenameAndSave(string newName)
        {
            GameBox gbx;

            try
            {
                gbx = GameBox.Parse(FullPath);
            }
            catch (Exception e)
            {
                throw e;
            }

            if (gbx is GameBox<CGameCtnChallenge> gbxMap)
            {
                CGameCtnChallenge challenge = gbxMap.Node;
                challenge.MapName = newName;
                gbxMap.Save(FullPath);
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
            if (defaultname is null)
                return null;
            string formattedName = defaultname;
            formattedName = TmEssentials.TextFormatter.Deformat(formattedName);
            return formattedName;
        }


        public void OpenMap(GbxGame selGame)
        {
            if (selGame is CustomGbxGame cgg)
                if (cgg.IsUnlimiter)
                {
                    Task.Run(() => OpenMapUnlimiter(selGame));
                    return;
                }


            ProcessStartInfo gameGbxStartInfo = new ProcessStartInfo(selGame.ExeLocation, "/useexedir /singleinst /file=\"" + FullPath + "\"");
            Process gameGbx = new Process();
            gameGbx.StartInfo = gameGbxStartInfo;
            gameGbx.Start();
        }

        async Task OpenMapUnlimiter(GbxGame selGame)
        {
            string exeName = "TmForever.exe";
            bool isRunning = ProcessManager.IsRunning(exeName);

            if (!isRunning) { 
                //start the unlimiter first
                await Task.Run(() => selGame.Launch());
                string unlimiterExeName = selGame.ExeLocation.Replace(selGame.InstalationFolder + "\\", "");
                while (ProcessManager.IsRunning(unlimiterExeName) == true)
                {
                    await Task.Delay(50);
                }
            }
            else //show msg about running game
                Console.WriteLine("An instance of TMUF is running already");

            ProcessStartInfo gameGbxStartInfo = new ProcessStartInfo((selGame.InstalationFolder + "\\"+ exeName), "/useexedir /singleinst /file=\"" + FullPath + "\"");
            Process gameGbx = new Process();
            gameGbxStartInfo.WorkingDirectory = selGame.InstalationFolder; //to avoid exe not found message
            gameGbx.StartInfo = gameGbxStartInfo;
            gameGbx.Start();
        }

    }
}
