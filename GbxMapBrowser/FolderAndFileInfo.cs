using System;
using System.Collections.Generic;
using System.Text;

namespace GbxMapBrowser
{
    public class FolderAndFileInfo
    {
        public string Name { get; internal set; }
        public string FullPath { get; internal set; }
        public double Size { get; internal set; }
        public string SizeString
        {
            get
            {
                return FileOperations.SizeToString(Size);
            }
        }
        public DateTime DateModified { get; internal set; }
        public string DateModifiedString
        {
            get
            {
                if (DateModified == null) return "NO DATE";
                return DateModified.ToString();
            }
        }
        public DateTime DateCreated { get; internal set; }
        public string DateCreatedString
        {
            get
            {
                if (DateCreated == null) return "NO DATE";
                return DateCreated.ToString();
            }
        }
    }
}
