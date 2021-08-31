using System;
using System.Collections.Generic;
using System.Linq;

namespace GbxMapBrowser
{
    class FolderInfo
    {
        public string FolderName { get; set; }
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
        } 
    }
}
