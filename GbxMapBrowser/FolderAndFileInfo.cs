using System;
using System.Threading.Tasks;

namespace GbxMapBrowser
{
    public abstract class FolderAndFileInfo
    {
        public string DisplayName { get; internal set; }
        public string OriginalName { get; internal set; }

        public string FullPath { get; internal set; }
        public Uri ImageSmall { get; internal set; }

        public double Size { get; internal set; }
        public string SizeString
        {
            get => FileOperations.SizeToString(Size);
        }
        public DateTime DateModified { get; internal set; }
        public string DateModifiedString => DateModified == DateTime.MinValue ? "NO DATE" : DateModified.ToString();
        public DateTime DateCreated { get; internal set; }
        public string DateCreatedString => DateModified == DateTime.MinValue ? "NO DATE" : DateCreated.ToString();

        public abstract Task DeleteAsync();
    }
}
