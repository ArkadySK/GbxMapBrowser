using System;

namespace GbxMapBrowser
{
    public class FolderAndFileInfo
    {
        public string DisplayName { get; internal set; }
        public string OriginalName { get; internal set; }

        public string FullPath { get; internal set; }
        public Uri ImageSmall { get; internal set; }

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
                return DateModified == DateTime.MinValue ? "NO DATE" : DateModified.ToString();
            }
        }
        public DateTime DateCreated { get; internal set; }
        public string DateCreatedString
        {
            get
            {
                return DateModified == DateTime.MinValue ? "NO DATE" : DateCreated.ToString();
            }
        }
    }
}
