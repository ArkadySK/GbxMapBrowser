using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.MwFoundations;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GbxMapBrowser
{
    public class MapInfo : FolderAndFileInfo
    {
        public string Author { get; private set; }
        public string CopperPrice { get; private set; }
        public string MapType { get; private set; }
        public string Titlepack { get; private set; }
        public Uri MoodIcon { get; private set; }
        public string Description { get; private set; }
        public string ObjectiveBronze { get; private set; }
        public string ObjectiveSilver { get; private set; }
        public string ObjectiveGold { get; private set; }
        public string ObjectiveAuthor { get; private set; }
        public ImageSource MapThumbnail { get; private set; }
        public bool IsWorking { get; }

        private readonly string _shortName;

        public MapInfo(string fullnamepath, bool basicInfoOnly)
        {
            CMwNod gbx;
            _shortName = fullnamepath.Split('\\').Last();
            FullPath = fullnamepath;
            FileInfo mapfileInfo = new(fullnamepath);
            DateModified = mapfileInfo.LastWriteTime;
            DateCreated = mapfileInfo.CreationTime;
            Size = mapfileInfo.Length;

            try
            {
                gbx = basicInfoOnly ? Gbx.ParseHeaderNode(fullnamepath) : Gbx.ParseNode(fullnamepath);
                IsWorking = true;
            }
            catch
            {
                InitAsErrorFile();
                IsWorking = false;
                return;
            }

            if (gbx is CGameCtnChallenge challenge)
            {
                DisplayName = Utils.ToReadableText(challenge.MapName);
                OriginalName = challenge.MapName;
                Titlepack = challenge.TitleId;
                if (string.IsNullOrEmpty(OriginalName))
                {
                    DisplayName = "ERROR - Empty map (" + _shortName + ")";
                    ImageSmall = new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Error.png");
                    return;
                }
                Uri enviImagePath = EnviManager.GetEnvironmentImagePath(challenge.Collection, Titlepack);
                ImageSmall = File.Exists(enviImagePath.AbsolutePath)
                    ? enviImagePath
                    : new Uri(Environment.CurrentDirectory + "\\Data\\Environments\\Unknown.png");

                InitMapTypeAndObjectives(challenge);
                if (basicInfoOnly) return;

                if (!string.IsNullOrEmpty(challenge.Comments))
                    Description = Utils.ToReadableText(challenge.Comments);
                MoodIcon = MoodManager.GetMoodImagePath(challenge.Decoration.ToString());
                Author = string.IsNullOrEmpty(challenge.AuthorNickname) ? challenge.AuthorLogin : Utils.ToReadableText(challenge.AuthorNickname);
                CopperPrice = challenge.Cost.ToString();
                InitMapThumbnail(challenge);
            }
            else if (gbx is CGameCtnReplayRecord gbxReplay)
                InitReplay(gbxReplay, basicInfoOnly);
        }

        public void RenameAndSave(string newName)
        {
            Gbx gbx = Gbx.Parse(FullPath);

            if (gbx is Gbx<CGameCtnChallenge> gbxMap)
            {
                CGameCtnChallenge challenge = gbxMap.Node;
                challenge.MapName = newName;
                gbxMap.Save(FullPath);
            }
            else
                throw new NotImplementedException("Only Maps could be renamed.");

        }

        public override async Task DeleteAsync()
        {
            await Task.Run(() => FileOperations.DeleteFile(FullPath));
        }

        public void OpenMap(GbxGame selGame)
        {
            if (selGame is CustomGbxGame cgg)
                if (cgg.IsUnlimiter)
                {
                    Task.Run(() => OpenMapUnlimiterAsync(selGame));
                    return;
                }


            ProcessStartInfo gameGbxStartInfo = new(selGame.ExeLocation, "/useexedir /singleinst /file=\"" + FullPath + "\"");
            Process gameGbx = new()
            {
                StartInfo = gameGbxStartInfo
            };
            gameGbx.Start();
        }

        internal async Task ExportThumbnailAsync(string filePath)
        {
            var dirPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            var img = MapThumbnail as BitmapImage;
            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(img));

            using var fileStream = new FileStream(filePath, FileMode.Create);
            encoder.Save(fileStream);
            await Task.CompletedTask;
        }

        private void InitAsErrorFile()
        {
            DisplayName = "ERROR (" + _shortName + ")";
            ImageSmall = new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Error.png");
            MapThumbnail = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Error.png"));
            MapThumbnail.Freeze();
        }

        private void InitMapThumbnail(CGameCtnChallenge challenge)
        {
            if (challenge.Thumbnail == null)
            {
                MapThumbnail = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\NoThumbnail.png"));
                MapThumbnail.Freeze();
                return;
            }
            var thumbnailMemoryStream = new MemoryStream(challenge.Thumbnail) ?? throw new Exception("buffer is empty");
            Bitmap mapThumbnail = new(new StreamReader(thumbnailMemoryStream).BaseStream);
            mapThumbnail.RotateFlip(RotateFlipType.Rotate180FlipX);
            MapThumbnail = Utils.ConvertToImageSource(mapThumbnail);
            MapThumbnail.Freeze();
        }

        private void InitReplay(CGameCtnReplayRecord gbxReplay, bool basicInfoOnly)
        {
            CGameCtnReplayRecord replay = gbxReplay;
            OriginalName = _shortName;
            ImageSmall = new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Replay.png");
            DisplayName = _shortName.Replace(".Replay.Gbx", "", StringComparison.OrdinalIgnoreCase);
            ObjectiveGold = Utils.TimeSpanToString(replay.Time);
            if (basicInfoOnly) return;
            ObjectiveAuthor = ObjectiveGold;
            ObjectiveGold = null;
            MapThumbnail = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Replay.png"));
            MapThumbnail.Freeze();
            Author = Utils.ToReadableText(replay.AuthorNickname);
            Titlepack = replay.TitleId;
        }

        private void InitMapTypeAndObjectives(CGameCtnChallenge challenge)
        {
            MapType = string.IsNullOrEmpty(challenge.ChallengeParameters?.MapType)
                ? challenge.Mode.ToString()
                : challenge.ChallengeParameters.MapType;

            bool isRace = challenge.Mode == CGameCtnChallenge.PlayMode.Race || MapType.EndsWith("Race");

            ObjectiveAuthor = !string.IsNullOrEmpty(challenge.ObjectiveTextAuthor) && !isRace
                ? challenge.ObjectiveTextAuthor
                : Utils.TimeSpanToString(challenge.AuthorTime);
            ObjectiveGold = !string.IsNullOrEmpty(challenge.ObjectiveTextGold) && !isRace
                ? challenge.ObjectiveTextGold
                : Utils.TimeSpanToString(challenge.GoldTime);
            ObjectiveSilver = !string.IsNullOrEmpty(challenge.ObjectiveTextSilver) && !isRace
                ? challenge.ObjectiveTextSilver
                : Utils.TimeSpanToString(challenge.SilverTime);
            ObjectiveBronze = !string.IsNullOrEmpty(challenge.ObjectiveTextBronze) && !isRace
                ? challenge.ObjectiveTextBronze
                : Utils.TimeSpanToString(challenge.BronzeTime);
        }

        private async Task OpenMapUnlimiterAsync(GbxGame selGame)
        {
            string exeName = "TmForever.exe";
            bool isRunning = ProcessManager.IsRunning(exeName);

            if (!isRunning)
            {
                //start the unlimiter first
                await Task.Run(selGame.Launch);
                string unlimiterExeName = selGame.ExeLocation.Replace(selGame.InstalationFolder + '\\', "");
                while (ProcessManager.IsRunning(unlimiterExeName) == true)
                {
                    await Task.Delay(50);
                }
            }
            else //show msg about running game
                Console.WriteLine("An instance of TMUF is running already");

            ProcessStartInfo gameGbxStartInfo = new(selGame.InstalationFolder + '\\' + exeName, "/useexedir /singleinst /file=\"" + FullPath + "\"");
            Process gameGbx = new();
            gameGbxStartInfo.WorkingDirectory = selGame.InstalationFolder; //to avoid exe not found message
            gameGbx.StartInfo = gameGbxStartInfo;
            gameGbx.Start();
        }
    }
}
