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
        public ImageSource MapThumbnail { get; }
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
            catch (Exception e)
            {
                DisplayName = "ERROR (" + _shortName + ")";
                ImageSmall = new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Error.png");
                Debug.WriteLine("Error: Map '" + fullnamepath + "' - impossible to load" + Environment.NewLine + e.Message);
                MapThumbnail = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Error.png"));
                MapThumbnail.Freeze();
                IsWorking = false;
                return;
            }

            if (gbx is CGameCtnChallenge gbxMap)
            {
                CGameCtnChallenge challenge = gbxMap;

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

                ObjectiveGold = Utils.TimeSpanToString(challenge.GoldTime);
                if (basicInfoOnly) return;
                ObjectiveBronze = Utils.TimeSpanToString(challenge.BronzeTime);
                ObjectiveSilver = Utils.TimeSpanToString(challenge.SilverTime);
                ObjectiveAuthor = Utils.TimeSpanToString(challenge.AuthorTime);

                if (!string.IsNullOrEmpty(challenge.Comments))
                    Description = Utils.ToReadableText(challenge.Comments);
                MoodIcon = MoodManager.GetMoodImagePath(challenge.Decoration.ToString());

                Author = string.IsNullOrEmpty(challenge.AuthorNickname) ? challenge.AuthorLogin : Utils.ToReadableText(challenge.AuthorNickname);

                CopperPrice = challenge.Cost.ToString();

                MapType = string.IsNullOrEmpty(challenge.ChallengeParameters.MapType)
                    ? challenge.Mode.ToString()
                    : challenge.ChallengeParameters.MapType;

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
            else if (gbx is CGameCtnReplayRecord gbxReplay)
            {
                CGameCtnReplayRecord replay = gbxReplay;
                OriginalName = _shortName;
                ImageSmall = new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Replay.png");
                DisplayName = _shortName.Replace(".Replay.Gbx", "", StringComparison.OrdinalIgnoreCase);
                ObjectiveGold = Utils.TimeSpanToString(replay.Time);
                if (basicInfoOnly) return;
                MapThumbnail = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Replay.png"));
                MapThumbnail.Freeze();
                Author = Utils.ToReadableText(replay.AuthorNickname);
                Titlepack = replay.TitleId;
            }
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

        private async Task OpenMapUnlimiterAsync(GbxGame selGame)
        {
            string exeName = "TmForever.exe";
            bool isRunning = ProcessManager.IsRunning(exeName);

            if (!isRunning)
            {
                //start the unlimiter first
                await Task.Run(() => selGame.Launch());
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
