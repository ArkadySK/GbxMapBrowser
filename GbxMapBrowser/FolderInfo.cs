using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace GbxMapBrowser
{
    class FolderInfo: FolderAndFileInfo
    {
        
        

        public FolderInfo(string fullnamepath)
        {
            Name = fullnamepath.Split('\\').LastOrDefault();
            FullPath = fullnamepath;
            DirectoryInfo directoryInfo = new DirectoryInfo(fullnamepath);
            DateModified = directoryInfo.LastWriteTime;
            DateCreated = directoryInfo.CreationTime;

        }
    }
}
