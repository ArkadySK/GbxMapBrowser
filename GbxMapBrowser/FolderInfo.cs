using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GbxMapBrowser
{
    class FolderInfo
    {
        public string FolderName { get; set; }
        public string FolderFullPath { get; set; }

        public FolderInfo(string fullnamepath)
        {
            FolderName = fullnamepath.Split('\\').LastOrDefault();
            FolderFullPath = fullnamepath;
        } 
    }
}
