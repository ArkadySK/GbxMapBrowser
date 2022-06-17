using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace GbxMapBrowser
{
    class FolderInfo: FolderAndFileInfo
    {
        public int FilesInsideCount { get; private set; }
        

        public FolderInfo(string fullnamepath)
        {
            Name = fullnamepath.Split('\\').LastOrDefault();
            FullPath = fullnamepath;
            DirectoryInfo directoryInfo = new DirectoryInfo(fullnamepath);
            DateModified = directoryInfo.LastWriteTime;
            DateCreated = directoryInfo.CreationTime;
            ImageSmall = new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Folder.png");
            FilesInsideCount = GetFilesCount();
        }

        int GetFilesCount()
        {
            if(!Directory.Exists(FullPath)) return 0;
            try
            {
                return Directory.GetFiles(FullPath).Length;
            }
            catch { return 0; }
        }
    }
}
