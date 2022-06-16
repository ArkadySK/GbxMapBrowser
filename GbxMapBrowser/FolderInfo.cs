using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace GbxMapBrowser
{
    class FolderInfo: FolderAndFileInfo
    {
        public string FolderName { get; }
        public double FolderSize { get; }
        public string FolderSizeString { 
            get
            {
                return FileOperations.SizeToString(FolderSize);
            }
        }
        public string FolderFullPath { get; }
        public DateTime DateModified { get; }
        public string DateModifiedString
        {
            get
            {
                if (DateModified == null) return "NO DATE";
                return DateModified.ToString();
            }
        }

        public FolderInfo(string fullnamepath)
        {
            FolderName = fullnamepath.Split('\\').LastOrDefault();
            FolderFullPath = fullnamepath;
            DirectoryInfo directoryInfo = new DirectoryInfo(fullnamepath);
            DateModified = directoryInfo.LastWriteTime;
        } 
    }
}
