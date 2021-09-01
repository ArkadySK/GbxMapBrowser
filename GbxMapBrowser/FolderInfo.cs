using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace GbxMapBrowser
{
    class FolderInfo
    {
        public string FolderName { get; set; }
        public double FolderSize { get; set; }
        public string FolderSizeString { 
            get
            {
                return FileOperations.SizeToString(FolderSize);
            }
        }
        public string FolderFullPath { get; set; }
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
