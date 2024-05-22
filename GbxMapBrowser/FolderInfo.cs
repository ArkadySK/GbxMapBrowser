using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GbxMapBrowser
{
    internal class FolderInfo : FolderAndFileInfo
    {
        public int FilesInsideCount
        {
            get
            {
                return GetFilesCount();
            }
        }
        public int MapsInsideCount
        {
            get
            {
                return GetMapsCount();
            }
        }

        public override async Task DeleteAsync()
        {
            await Task.Run(() => Directory.Delete(FullPath, true));
        }

        public FolderInfo(string fullnamepath)
        {
            DisplayName = fullnamepath.Split('\\').LastOrDefault();
            OriginalName = fullnamepath;
            FullPath = fullnamepath;
            DirectoryInfo directoryInfo = new(fullnamepath);
            DateModified = directoryInfo.LastWriteTime;
            DateCreated = directoryInfo.CreationTime;
            ImageSmall = new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Folder.png");
        }

        private int GetFilesCount()
        {
            if (!Directory.Exists(FullPath)) return 0;
            try
            {
                return Directory.GetFiles(FullPath).Length;
            }
            catch { return 0; }
        }

        private int GetMapsCount()
        {
            if (!Directory.Exists(FullPath)) return 0;
            try
            {
                var files = Directory.GetFiles(FullPath);
                var maps = files.Where(x => x.EndsWith(".Gbx")).ToArray();
                return maps.Length;
            }
            catch { }
            return 0;
        }
    }
}
